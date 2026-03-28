using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
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
	public required ConcurrentDictionary<string, SharpIdeFile> AllFiles { get; set; }
	public required HashSet<SharpIdeFolder> AllFolders { get; set; } // TODO: this isn't thread safe
	public bool Expanded { get; set; }

	[SetsRequiredMembers]
	internal SharpIdeSolutionModel(string solutionFilePath, IntermediateSolutionModel intermediateModel)
	{
		var solutionName = Path.GetFileName(solutionFilePath);
		var allProjects = new ConcurrentBag<SharpIdeProjectModel>();
		var allFiles = new ConcurrentBag<SharpIdeFile>();
		var allFolders = new ConcurrentBag<SharpIdeFolder>();
		Name = solutionName;
		FilePath = solutionFilePath;
		DirectoryPath = Path.GetDirectoryName(solutionFilePath)!;
		Projects = new ObservableHashSet<SharpIdeProjectModel>(intermediateModel.Projects.Select(s => new SharpIdeProjectModel(s, allProjects, allFiles, allFolders, this)));
		SlnFolders = new ObservableHashSet<SharpIdeSolutionFolder>(intermediateModel.SolutionFolders.Select(s => new SharpIdeSolutionFolder(s, allProjects, allFiles, allFolders, this)));
		AllProjects = allProjects.ToHashSet();
		AllFiles = new ConcurrentDictionary<string, SharpIdeFile>(allFiles.DistinctBy(s => s.Path).ToDictionary(s => s.Path));
		AllFolders = allFolders.ToHashSet();
	}
}
