using System.Collections.Generic;
using System.IO;

namespace FYPSampleApplication;

public class FileReassembler
{
    public static void AssembleFile(IEnumerable<FileChunk> chunks, string destinationPath)
    {
        using (FileStream fileStream = new(destinationPath, FileMode.Create))
        {
            foreach (FileChunk chunk in chunks)
            {
                fileStream.Write(chunk.Data, 0, chunk.Length);
            }
        }
    }
}