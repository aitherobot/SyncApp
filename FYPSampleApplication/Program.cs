using System;
using System.IO;
using System.Threading;

namespace FYPSampleApplication;

class Program
{
    private static void Main(string[] args)
    {
        string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string syncFolder = Path.Combine(documentsFolder, "SyncTest");
        Directory.CreateDirectory(syncFolder);
        
        var discovery = new PeerDiscovery();
        var transfer = new PeerTransfer(syncFolder);

        discovery.StartListening();
        transfer.StartServer();
        
        var watcher = new FolderWatcher(syncFolder, discovery, transfer);
        watcher.Start();
        transfer.Watcher = watcher;

        Console.WriteLine("Watching Folder. Press Enter tp broadcast presence");
        Console.ReadLine();

        discovery.StartBroadcasting();
        
        Console.WriteLine("Now modify or create a file in the sync folder and watch the output. Press Ctrl+C to exit.");
        while (true) Thread.Sleep(1000);
        
        #region Old Tests
        
        // // create test diffs to send
        // var testDiffs = new List<ChunkDiff>
        // {
        //     
        //     new ChunkDiff { Index = 1, Data = new byte[512 * 1024]},
        //     new ChunkDiff { Index = 3, Data = new byte[256 * 1024]}
        // };
        //
        // transfer.SendDiffs("127.0.0.1", testDiffs);
        //
        // Console.WriteLine("Broadcasting sent. Waiting 2 seconds to exit");
        // Thread.Sleep(2000);
        //
        // Console.WriteLine($"Discovered peers: {string.Join(", ", discovery.DiscoveredPeers)}");
        // Console.WriteLine("Done. Press Enter to exit.");
        // Console.ReadLine();
        
        
        /* var chunker = new FileChunker();
        var hasher = new FileHasher();

        var filePath = @"C:\Users\iayot\Documents\Ayotunde\Videos\Merkle Tree Test.txt";
        var outputPath = @"C:\Users\iayot\Documents\Ayotunde\Videos\Animes\Shingeki no Kyojin\s1\ep 22_copy.mp4";

        string fileA = "testA.txt";
        string fileB = "testB.txt";

        File.WriteAllBytes(fileA, new byte[3 * 1024 * 1024].Select(_ => (byte)'A').ToArray());

        byte[] fileBData = File.ReadAllBytes(fileA);
        fileBData[512 * 1024 + 5] = (byte)'Z';
        fileBData[3072 * 1024 - 5] = (byte)'Y';
        File.WriteAllBytes(fileB, fileBData);

        //fileBData = File.ReadAllBytes(fileA);
        //fileBData[2 * 1024 * 1024 + 10] = (byte)'Y';
        //File.WriteAllBytes(fileB, fileBData);

        var chunksA = chunker.GetChunks(fileA).ToList();
        var chunksB = chunker.GetChunks(fileB).ToList();

        var treeA = new MerkleTree(chunksA, hasher);
        var treeB = new MerkleTree(chunksB, hasher);

        var changedIndices = treeA.GetChangedLeafsIndices(treeB);

        Console.WriteLine($"Changed chunk indices: {string.Join(", ", changedIndices)}");

        var sync = new DifferentialSync();
        List<ChunkDiff> diffs = sync.ComputeDiff(changedIndices, chunksB);

        Console.WriteLine($"Chunks to transfer: {diffs.Count}");
        foreach (ChunkDiff diff in diffs)
        {
            Console.WriteLine($"Chunk index: {diff.Index}: {diff.Data.Length} bytes");
        }


        //List<FileChunk> chunks = chunker.GetChunks(filePath).ToList();

        //foreach (FileChunk chunk in chunks)
        //{
        //    byte[] hash = hasher.HashChunk(chunk);

        //    Console.WriteLine($"{chunk.Index}, {chunk.Length}, {hasher.ToHexString(hash)}");
        //}

        //var tree = new MerkleTree(chunks);
        //Console.WriteLine($"Root hash: {hasher.ToHexString(tree.Root.Hash)}"); */

        #endregion
    }
}