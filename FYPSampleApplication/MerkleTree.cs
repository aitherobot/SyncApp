using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYPSampleApplication;

public class MerkleTree
{
    private readonly FileHasher _hasher = new();

    public MerkleNode Root { get; private set; }

    public MerkleTree(List<FileChunk> chunks, FileHasher hasher)
    {
        if (chunks == null) throw new ArgumentNullException(nameof(chunks));
        if (hasher == null) throw new ArgumentNullException(nameof(hasher));

        _hasher = hasher;

        var nodes = chunks.Select(chunk =>
            new MerkleNode(_hasher.HashChunk(chunk))).ToList();

        while (nodes.Count > 1)
        {
            var parentNodes = new List<MerkleNode>();

            for (int i = 0; i < nodes.Count; i += 2)
            {
                var left = nodes[i];
                var right = (i + 1 < nodes.Count) ? nodes[i + 1] : left;

                byte[] combinedHash = _hasher.HashPair(left.Hash, right.Hash);

                parentNodes.Add(new MerkleNode(combinedHash, left, right));
            }

            nodes = parentNodes;
        }

        Root = nodes[0];
    }

    private static bool HashesEqual(byte[] leftHash, byte[] rightHash) =>
        leftHash.AsSpan().SequenceEqual(rightHash);

    private static int CountLeaves(MerkleNode node)
    {
        if (node.IsLeaf)
        {
            return 1;
        }

        int count = 0;

        if (node.Left is not null)
        {
            count += CountLeaves(node.Left);
        }

        if (node.Right is not null)
        {
            count += CountLeaves(node.Right);
        }

        return count;
    }

    private void GetChangedLeafs(MerkleNode? left, MerkleNode? right, List<int> changedLeafs, ref int leafIndex)
    {
        // If both are null, nothing to do (shouldn't happen in a valid tree traversal)
        if (left == null && right == null) return;

        // If one is null, all leaves under the other are "changed" (added or removed)
        if (left == null)
        {
            AddAllLeaves(right!, changedLeafs, ref leafIndex);
            return;
        }

        if (right == null)
        {
            AddAllLeaves(left, changedLeafs, ref leafIndex); // Indices in left tree
            return;
        }

        // If hashes are equal, skip this branch
        if (HashesEqual(left.Hash, right.Hash))
        {
            leafIndex += CountLeaves(left);
            return;
        }

        // If both are leaves and hashes differ
        if (left.IsLeaf && right.IsLeaf)
        {
            changedLeafs.Add(leafIndex);
            leafIndex++;
            return;
        }

        // Recursively check children. 
        // Note: Merkle tree construction here always has both children or none, 
        // except possibly when the tree is lopsided, but our construction 
        // handles right = left if odd, so nodes always have two children if not leaf.
        GetChangedLeafs(left.Left, right.Left, changedLeafs, ref leafIndex);
        GetChangedLeafs(left.Right, right.Right, changedLeafs, ref leafIndex);
    }

    private void AddAllLeaves(MerkleNode node, List<int> changedLeafs, ref int leafIndex)
    {
        if (node.IsLeaf)
        {
            changedLeafs.Add(leafIndex);
            leafIndex++;
            return;
        }
        if (node.Left != null) AddAllLeaves(node.Left, changedLeafs, ref leafIndex);
        if (node.Right != null) AddAllLeaves(node.Right, changedLeafs, ref leafIndex);
    }

    public List<int> GetChangedLeafsIndices(MerkleTree other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var changedLeaves = new List<int>();
        int leafIndex = 0;
        GetChangedLeafs(this.Root, other.Root, changedLeaves, ref leafIndex);
        return changedLeaves;
    }
}