using System;
using System.Collections.Generic;
using System.IO;

namespace FYPSampleApplication;

public class FileChunker
{
    private readonly int _chunkSize;

    public FileChunker(int chunkSize = 512 * 1024)
    {
        if (chunkSize <= 0) throw new ArgumentException("Chunk size must be greater than zero.", nameof(chunkSize));
        this._chunkSize = chunkSize;
    }

    /// <summary>
    /// Splits the specified file into a sequence of chunks based on the configured chunk size.
    /// </summary>
    /// <param name="filePath">The path of the file to be chunked.</param>
    /// <returns>An enumerable collection of <see cref="FileChunk"/> objects representing the chunks of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the chunk size is invalid.</exception>

    public IEnumerable<FileChunk> GetChunks(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);
        
        using (FileStream stream = File.OpenRead(filePath))
        {
            var buffer = new byte[_chunkSize];
            int chunkIndex = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var chunkData = new byte[bytesRead];
                Array.Copy(buffer, chunkData, bytesRead);
                
                yield return new FileChunk(chunkIndex, chunkData);
                chunkIndex++;
            }
        }
    }
}