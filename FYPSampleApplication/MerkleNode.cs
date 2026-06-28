using System;

namespace FYPSampleApplication;

public class MerkleNode
{
    public byte[] Hash { get; init; }
    public MerkleNode? Left { get; init; }
    public MerkleNode? Right { get; init; }
    public bool IsLeaf => Left == null && Right == null;

    public MerkleNode(byte[] hash, MerkleNode? left = null, MerkleNode? right = null)
    {
        if (hash == null) throw new ArgumentNullException(nameof(hash));
        if (hash.Length == 0) throw new ArgumentException("Hash cannot be empty", nameof(hash));
        
        Hash = hash;
        Left = left;
        Right = right;
    }
}