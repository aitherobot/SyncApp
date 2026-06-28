# FYPSampleApplication Agent Guidelines

## Architecture Overview
This is a .NET 10 console application for efficient file synchronization using chunking, hashing, and Merkle trees. It enables peer discovery and differential synchronization between files.

### Key Components:
- **FileChunker**: Splits files into configurable chunks (default 512KB). Uses `yield return` for streaming.
- **FileHasher**: Computes SHA256 hashes for chunks and combined hash pairs for the Merkle tree.
- **MerkleTree**: Builds a binary hash tree from chunks to efficiently identify differences between files.
- **DifferentialSync**: Computes the minimum set of `ChunkDiff` objects needed to synchronize two files based on Merkle tree comparison.
- **PeerDiscovery**: Implements UDP broadcasting/listening for discovering other synchronization nodes (currently skeleton).
- **FileReassembler**: Reconstructs files from a collection of chunks.
- **Data Structures**: `FileChunk` and `ChunkDiff` are immutable records/classes using `init` properties.

## Key Patterns
- **Immutability**: Prefer `init` properties and immutable data structures for chunk representation.
- **Error Handling**: Throw `ArgumentException` for invalid parameters and `FileNotFoundException` for missing resources.
- **Memory Efficiency**: Use `IEnumerable` and `yield return` in `FileChunker` to avoid loading entire files into memory.
- **Modern C#**: Use file-scoped namespaces, collection expressions (`[..left, ..right]`), and `Span<T>` for efficient memory operations where appropriate (e.g., hash comparison).
- **Static Utilities**: Use `HashUtils` for generic string-based hashing and `FileHasher` for byte-array based operations.

## Development Workflow
- **Build**: `dotnet build`
- **Run**: `dotnet run` (Program.cs contains a demonstration of differential sync between two generated test files).
- **Testing**: Manual verification via `Program.cs` console output showing changed indices and transfer sizes.

## Conventions
- **Namespaces**: Single `FYPSampleApplication` namespace; use file-scoped declarations (`namespace FYPSampleApplication;`).
- **Naming**: PascalCase for classes/methods, camelCase for local variables and private fields (with `_` prefix).
- **Nullability**: Enabled; explicitly handle or check for nulls.
- **Paths**: Use verbatim strings (`@""`) for hardcoded Windows paths in test code.

## Integration Points
- **I/O**: `System.IO` for file streams and binary access.
- **Security**: `System.Security.Cryptography.SHA256` for data integrity.
- **Networking**: `System.Net.Sockets` (planned for `PeerDiscovery`) for UDP synchronization.

## Examples
- **Merkle Tree Comparison**:
  ```csharp
  var treeA = new MerkleTree(chunksA, hasher);
  var treeB = new MerkleTree(chunksB, hasher);
  var changedIndices = treeA.GetChangedLeafsIndices(treeB);
  ```
- **Differential Sync**:
  ```csharp
  var sync = new DifferentialSync();
  List<ChunkDiff> diffs = sync.ComputeDiff(changedIndices, chunksB);
  ```
- **Chunking**: `var chunks = new FileChunker().GetChunks(path);`

Reference: `Program.cs` for full integration flow, `MerkleTree.cs` for tree logic, `DifferentialSync.cs` for diff computation.
