using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace FYPSampleApplication;

public class FolderWatcher
{
    private readonly string _folderPath;
    private readonly FileChunker _chunker = new ();
    private readonly FileHasher _hasher = new ();
    private readonly Dictionary<string, MerkleTree> _previousTrees = new ();
    private readonly Dictionary<string, Timer> _debounceTimers = new ();
    private readonly PeerDiscovery _discovery;
    private readonly PeerTransfer _transfer;
    private FileSystemWatcher? _watcher;
    private readonly Action<string> _log;

    private static readonly string SnapshotPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "P2PFileSync", "snapshot.json");
    
    public FolderWatcher(string folderPath, PeerDiscovery discovery, PeerTransfer transfer, Action<string> log = null)
    {
        _folderPath = folderPath;
        _discovery = discovery;
        _transfer = transfer;
        _log = log ?? Console.WriteLine;
    }

    public void Start()
    {
        StartupScan();
        
        _watcher = new FileSystemWatcher(_folderPath)
        {
            NotifyFilter = NotifyFilters.FileName 
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Size,
            Filter = "*.*",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += (s, e) => OnFileChanged(s, e);
    }

    private void StartupScan()
    {
        var snapShot = LoadSnapshot();
        var files = Directory.GetFiles(_folderPath, "*.*", SearchOption.AllDirectories);
        
        foreach (var filePath in files)
        {
            try
            {
                var chunks = _chunker.GetChunks(filePath).ToList();
                var tree = new MerkleTree(chunks, _hasher);
                _previousTrees[filePath] = tree;
                
                string currentRoot = Convert.ToHexString(tree.Root.Hash);

                if (snapShot.TryGetValue(filePath, out string? savedRoot) && savedRoot != currentRoot)
                {
                    _log($"[Startup] Offline edit detected: {Path.GetFileName(filePath)}, queing sync...");
                    
                    var diffs = chunks
                        .Select(c => new ChunkDiff {Index = c.Index, Data = c.Data})
                        .ToList();
                    
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        foreach (var peer in _discovery.DiscoveredPeers)
                            _transfer.SendDiffs(peer, filePath, diffs);
                    });
                }
                else
                {
                    _log($"[Startup] Indexed: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex)
            {
                _log($"[Startup] Could not index '{Path.GetFileName(filePath)}': {ex.Message}");
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (_debounceTimers.TryGetValue(e.FullPath, out Timer? existing))
        {
            existing.Stop();
            existing.Start();
            return;
        }
        
        var timer = new Timer(500) { AutoReset = false };
        timer.Elapsed += (_, _) =>
        {
            _debounceTimers.Remove(e.FullPath);
            ProcessFileChange(e.FullPath);
        };
        _debounceTimers[e.FullPath] = timer;
        timer.Start();
    }

    private void ProcessFileChange(string filePath)
    {
        _log($"[DEBUG] Processing change for: {filePath}");

        if (!File.Exists(filePath))
        {
            _log($"[INFO] File '{filePath}' was deleted or moved.");
            _previousTrees.Remove(filePath);
            return;
        }

        // 1. chunk and hash the new version of the file
        List<FileChunk> chunks;
        try
        {
            chunks = _chunker.GetChunks(filePath).ToList();
        }
        catch (Exception ex)
        {
            _log($"[WARN] Could not read file '{filePath}': {ex.Message}");
            return;
        }
        
        // 2. build a new MerkleTree from the chunks
        var newTree = new MerkleTree(chunks, _hasher);
        
        // 3. compare with the previous tree (if one exists)
        if (_previousTrees.TryGetValue(filePath, out MerkleTree? previousTree))
        {
            // 4. if changes found, compute diff and send to all peers
            List<int> changedIndices = previousTree.GetChangedLeafsIndices(newTree);

            if (changedIndices.Count > 0)
            {
                var diffs = changedIndices
                    .Where(i => i < chunks.Count)
                    .Select(i => new ChunkDiff()
                    {
                        Index = chunks[i].Index,
                        Data = chunks[i].Data
                    })
                    .ToList();

                _log(
                    $"[INFO] {diffs.Count} changed chunk(s) in '{Path.GetFileName(filePath)}', sending to peers...");

                foreach (var peer in _discovery.DiscoveredPeers)
                {
                        _transfer.SendDiffs(peer, filePath, diffs);
                }
            }
            else
            {
                _log($"[INFO] No changes detected in '{Path.GetFileName(filePath)}'");
            }
        }
        else
        {
            var diffs = chunks
                .Select(c => new ChunkDiff { Index = c.Index, Data = c.Data })
                .ToList();
            _log($"[INFO] New file '{Path.GetFileName(filePath)}' detected, sending to peers...");
                
            foreach (var peer in _discovery.DiscoveredPeers)
                _transfer.SendDiffs(peer, filePath, diffs);
        }
        
        // 5. store the new tree as the previous tree for next comparison
        _previousTrees[filePath] = newTree;
    }

    public void Pause() => _watcher!.EnableRaisingEvents = false;
    public void Resume() => _watcher!.EnableRaisingEvents = true;

    public void SaveSnapshot()
    {
        var snapshot = _previousTrees.ToDictionary(
            kvp => kvp.Key,
            kvp => Convert.ToHexString(kvp.Value.Root.Hash)
            );
        Directory.CreateDirectory(Path.GetDirectoryName(SnapshotPath)!);
        File.WriteAllText(SnapshotPath, System.Text.Json.JsonSerializer.Serialize(snapshot));
    }
    
    private Dictionary<string, string> LoadSnapshot()
    {
        try
        {
            if (File.Exists(SnapshotPath))
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(SnapshotPath)) ?? new ();
        }
        catch { }
        return new ();
    }
}