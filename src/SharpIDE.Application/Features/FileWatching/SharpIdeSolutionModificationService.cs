using SharpIDE.Application.Features.SolutionDiscovery;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.FileWatching;

/// Does not do any file system operations, only modifies the in-memory solution model
public class SharpIdeSolutionModificationService
{
	public SharpIdeSolutionModel SolutionModel { get; set; } = null!;

	/// The directory must already exist on disk
	public async Task<SharpIdeFolder> AddDirectory(SharpIdeFolder parentFolder, string directoryName)
	{
		// Passing [] to allFiles and allFolders, as we assume that a brand new folder has no subfolders or files yet
		var addedDirectoryPath = Path.Combine(parentFolder.Path, directoryName);
		var sharpIdeFolder = new SharpIdeFolder(new DirectoryInfo(addedDirectoryPath), parentFolder, [], []);
		parentFolder.Folders.Add(sharpIdeFolder);
		SolutionModel.AllFolders.Add(sharpIdeFolder);
		return sharpIdeFolder;
	}

	public async Task RemoveDirectory(SharpIdeFolder folder)
	{
		var parentFolderOrProject = (IFolderOrProject)folder.Parent;
		parentFolderOrProject.Folders.Remove(folder);
		SolutionModel.AllFolders.Remove(folder);
	}
}
