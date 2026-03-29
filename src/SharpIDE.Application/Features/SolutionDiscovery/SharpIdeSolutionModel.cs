using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
using SharpIDE.Application.Features.FileSystem;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.SolutionDiscovery;

public class SharpIdeSolutionModel : ISharpIdeNode, IExpandableSharpIdeNode, ISolutionOrProject
{
	public required string Name { get; set; }
	public required string FilePath { get; set; }
	public required string DirectoryPath { get; set; }
	public required ObservableHashSet<SharpIdeProjectModel> Projects { get; set; }
	public required ObservableHashSet<SharpIdeSolutionFolder> SlnFolders { get; set; }
	public required HashSet<SharpIdeProjectModel> AllProjects { get; set; } // TODO: this isn't thread safe
	public bool Expanded { get; set; }

	public required SharpIdeRootFolder RootFolder { get; set; }
	public ConcurrentDictionary<string, SharpIdeFile> AllFiles => RootFolder.AllFiles;
	public ObservableList<SharpIdeFolder> AllFolders => RootFolder.AllFolders;

	[SetsRequiredMembers]
	internal SharpIdeSolutionModel(string solutionFilePath, IntermediateSolutionModel intermediateModel, SharpIdeRootFolder sharpIdeRootFolder)
	{
		var solutionName = Path.GetFileName(solutionFilePath);
		var allProjects = new ConcurrentBag<SharpIdeProjectModel>();
		var allFiles = new ConcurrentBag<SharpIdeFile>();
		var allFolders = new ConcurrentBag<SharpIdeFolder>();
		Name = solutionName;
		FilePath = solutionFilePath;
		DirectoryPath = Path.GetDirectoryName(solutionFilePath)!;
		RootFolder = sharpIdeRootFolder;
		Projects = new ObservableHashSet<SharpIdeProjectModel>(intermediateModel.Projects.Select(s => new SharpIdeProjectModel(s, allProjects, allFiles, allFolders, this, sharpIdeRootFolder)));
		SlnFolders = new ObservableHashSet<SharpIdeSolutionFolder>(intermediateModel.SolutionFolders.Select(s => new SharpIdeSolutionFolder(s, allProjects, allFiles, allFolders, this, sharpIdeRootFolder)));
		AllProjects = allProjects.ToHashSet();
	}
}

public static class SharpIdeSolutionModelExtensions
{
	public static SharpIdeProjectModel? GetProjectForContainingFolderPath(this SharpIdeSolutionModel solution, SharpIdeFolder folder)
	{
		var sharpIdeProject = solution.AllProjects.SingleOrDefault(s => s.DirectoryPath == folder.Path);
		return sharpIdeProject;
	}
}
