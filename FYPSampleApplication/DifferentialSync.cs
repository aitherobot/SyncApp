using System.Collections.Generic;
using System.Linq;

namespace FYPSampleApplication;

public class DifferentialSync
{
    public List<ChunkDiff> ComputeDiff(List<int> changedIndices, List<FileChunk> newChunks)
    {
        return newChunks.Where(chunk => changedIndices.Contains(chunk.Index))
                 .Select(chunk => new ChunkDiff { Index = chunk.Index, Data = chunk.Data })
                 .ToList();
    }
}
