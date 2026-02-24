using Microsoft.CodeAnalysis;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Application.Features.Analysis;

public class SharpIdeMetadataAsSourceService(RoslynAnalysis roslynAnalysis)
{
	private readonly RoslynAnalysis _roslynAnalysis = roslynAnalysis;
	private readonly Dictionary<string, SharpIdeFile> _metadataAsSourceFileCache = [];

	public async Task<(SharpIdeFile, Location)?> CreateSharpIdeFileForMetadataAsSourceAsync(SharpIdeFile currentFile, ISymbol referencedSymbol)
	{
		var result = await _roslynAnalysis.GetMetadataAsSourceForSymbol(currentFile, referencedSymbol);
		if (result is null) return null;
		var (filePath, requestedSymbolLocation) = result.Value;
		var fileFromCache = _metadataAsSourceFileCache.GetValueOrDefault(filePath);
		if (fileFromCache is not null) return (fileFromCache, requestedSymbolLocation);
		var metadataAsSourceSharpIdeFile = new SharpIdeFile(filePath, Path.GetFileName(filePath), Path.GetExtension(filePath), null!, [], true);
		_metadataAsSourceFileCache[filePath] = metadataAsSourceSharpIdeFile;
		return (metadataAsSourceSharpIdeFile, requestedSymbolLocation);
	}

	public async Task<SharpIdeFile?> GetOrCreateSharpIdeFileForAlreadyDecompiledMetadataAsSourceAsync(string filePath)
	{
		var filePathOnDisk = await _roslynAnalysis.WriteSourceFromMetadataAsSourceWorkspaceToDisk(filePath);
		if (filePathOnDisk is null) return null;
		var sharpIdeFileFromCache = _metadataAsSourceFileCache.GetValueOrDefault(filePathOnDisk);
		if (sharpIdeFileFromCache is not null) return sharpIdeFileFromCache;
		var metadataAsSourceSharpIdeFile = new SharpIdeFile(filePathOnDisk, Path.GetFileName(filePathOnDisk), Path.GetExtension(filePathOnDisk), null!, [], true);
		_metadataAsSourceFileCache[filePathOnDisk] = metadataAsSourceSharpIdeFile;
		return metadataAsSourceSharpIdeFile;
	}

}
