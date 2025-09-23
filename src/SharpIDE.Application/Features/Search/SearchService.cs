using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.Search;

public static class SearchService
{
	public static async Task FindInFiles(SharpIdeSolutionModel solutionModel, string searchTerm)
	{
		if (searchTerm.Length < 4)
		{
			return;
		}
		var files = solutionModel.AllFiles;
		ConcurrentBag<string> results = [];
		await Parallel.ForEachAsync(files, async (file, ct) =>
			{
				await foreach (var (index, line) in File.ReadLinesAsync(file.Path, ct).Index().WithCancellation(ct))
				{
					if (line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
					{
						results.Add($"{file.Path} (Line {index + 1}): {line.Trim()}");
					}
				}
			}
		);
	}
}
