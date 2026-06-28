# P2PSync 🚀

P2PSync is a high-performance, peer-to-peer (P2P) file synchronization application designed for efficient data transfer across local area networks (LAN). By leveraging Merkle Trees and differential synchronization algorithms, P2PSync ensures that only the modified parts of files are transferred, significantly reducing bandwidth usage and synchronization time.

## ✨ Features

- **P2P Discovery**: Automatically finds other peers running P2PSync on your local network using UDP broadcasting.
- **Differential Sync**: Uses a chunking mechanism to identify specific changes within files, transferring only the "diffs" rather than the entire file.
- **Merkle Tree Validation**: Implements Merkle Trees to efficiently verify file integrity and quickly identify mismatched data blocks between peers.
- **Real-time Monitoring**: A background `FolderWatcher` tracks file system changes (creation, modification) and triggers sync events immediately.
- **Modern WPF Interface**: A clean and intuitive Windows Desktop application to monitor sync status, activity logs, and connected peers.
- **Cross-Project Architecture**:
    - **SyncApp**: The WPF-based graphical user interface.
    - **FYPSampleApplication**: The core logic library containing sync algorithms and networking components.

## 🛠️ Technology Stack

- **Language**: C# 14.0
- **Framework**: .NET 10.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Networking**: TCP for reliable file transfer, UDP for peer discovery.
- **Algorithms**: Merkle Tree hashing, Chunk-based Differential Sync.

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows OS (for WPF support)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/[Your-Username]/P2PSync.git
   cd P2PSync
   ```

2. **Build the solution**:
   ```bash
   dotnet build P2PSync.sln
   ```

### Usage

1. **Launch the Application**: Run the `SyncApp` project from Rider, Visual Studio, or via CLI:
   ```bash
   dotnet run --project SyncApp/SyncApp.csproj
   ```
2. **Select Sync Folder**: By default, it watches `Documents/SyncTest`. You can change this using the "Browse" button in the UI.
3. **Connect Peers**: Run P2PSync on another computer on the same network. They will discover each other automatically.
4. **Syncing**: Simply drop a file into the watched folder or modify an existing one. The app will calculate the Merkle hashes and sync the changes to other peers.

## 🧠 How it Works

1. **Discovery**: The app broadcasts its presence via UDP. Peers maintain a list of active IP addresses.
2. **Chunking**: Files are broken down into manageable chunks.
3. **Hashing**: Each chunk is hashed, and a Merkle Tree is constructed for the entire file.
4. **Comparison**: When a file changes, the local Merkle Tree is compared with the remote peer's tree to find the exact indices of modified chunks.
5. **Transfer**: Only the modified chunks are sent over a TCP stream and reassembled on the target machine.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
