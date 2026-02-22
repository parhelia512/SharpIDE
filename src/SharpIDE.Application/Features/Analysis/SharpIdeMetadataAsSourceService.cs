using Microsoft.CodeAnalysis;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Application.Features.Analysis;

public class SharpIdeMetadataAsSourceService(RoslynAnalysis roslynAnalysis)
{
	private readonly RoslynAnalysis _roslynAnalysis = roslynAnalysis;

	public async Task<SharpIdeFile?> CreateSharpIdeFileForMetadataAsSourceAsync(SharpIdeFile currentFile, ISymbol referencedSymbol)
	{
		var filePath = await _roslynAnalysis.GetMetadataAsSource(currentFile, referencedSymbol);
		if (filePath is null) return null;
		var metadataAsSourceSharpIdeFile = new SharpIdeFile(filePath, Path.GetFileName(filePath), Path.GetExtension(filePath), null!, [], true);
		return metadataAsSourceSharpIdeFile;
	}
}
