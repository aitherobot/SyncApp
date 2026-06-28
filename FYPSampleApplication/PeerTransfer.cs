using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FYPSampleApplication
{
    public class PeerTransfer
    {
        private const int ChunkSize = 512 * 1024; // 524288 bytes
        private const int TcpPort = 41235; // any unused port
        private readonly string _syncFolder;
        private readonly Action<string> _log;
        public FolderWatcher? Watcher { get; set; }

        public PeerTransfer(string syncFolder, Action<string>? log = null)
        {
            _syncFolder = syncFolder;
            _log = log ?? Console.WriteLine;
        }

        public void StartServer()
        {
            Task.Run(() =>
            {
                var listener = new TcpListener(IPAddress.Any, TcpPort);
                listener.Start();
                _log($"[Server] TCP listener started");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    string senderIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();
                    
                    using var stream = client.GetStream();
                    using var reader = new BinaryReader(stream);
                    
                    string fileName = reader.ReadString();
                    string localPath = Path.Combine(_syncFolder, Path.GetFileName(fileName));
                    
                    int count = reader.ReadInt32();

                    Watcher?.Pause();

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int index = reader.ReadInt32();
                            int dataLength = reader.ReadInt32();
                            byte[] data = reader.ReadBytes(dataLength);

                            fs.Seek(index * ChunkSize, SeekOrigin.Begin);
                            fs.Write(data, 0, data.Length);
                        }
                    }

                    Watcher?.Resume();
                    
                    _log($"[Receive] {count} chunk(s) of '{Path.GetFileName(fileName)}' from {senderIp}");
                }
            });
        }

        public void SendDiffs(string peerIp, string filePath, List<ChunkDiff> diffs)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(peerIp, TcpPort);
            
                using var stream = client.GetStream();
                using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
            
                writer.Write(filePath);
                writer.Write(diffs.Count);
            
                foreach (var diff in diffs)
                {
                    writer.Write(diff.Index);
                    writer.Write(diff.Data.Length);
                    writer.Write(diff.Data);
                }
            
                writer.Flush(); 
                _log($"[Send] {diffs.Count} chunk(s) of '{Path.GetFileName(filePath)}' to {peerIp}");
            }
            catch (Exception ex)
            {
                _log($"[Send] Failed to reach {peerIp}: {ex.Message}");
            }
        }
    }
}
