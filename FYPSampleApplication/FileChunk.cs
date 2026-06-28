using System;

namespace FYPSampleApplication;

public class FileChunk
{
    public int Index  { get; init; }
    public byte[] Data { get; init; }
    public int Length => Data.Length;
    
    public FileChunk(int index, byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) throw new ArgumentException("Chunk data cannot be empty", nameof(data));
        
        Index = index;
        Data = data;
    }
}