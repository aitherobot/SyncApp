using System;
using System.Security.Cryptography;

namespace FYPSampleApplication;

public class FileHasher
{
    public byte[] HashChunk(FileChunk chunk)
    {
        return SHA256.HashData(chunk.Data);
    }

    /// <summary>
    /// Computes the SHA256 hash of the combined data from two byte arrays.
    /// </summary>
    /// <param name="left">The first byte array to be combined and hashed.</param>
    /// <param name="right">The second byte array to be combined and hashed.</param>
    /// <returns>A byte array representing the SHA256 hash of the combined data.</returns>
    public byte[] HashPair(byte[] left, byte[] right)
    {
        byte[] combined = [..left, ..right];
        
        return SHA256.HashData(combined);
    }

    public string ToHexString(byte[] hash)
    {
        return Convert.ToHexString(hash);
    }
}