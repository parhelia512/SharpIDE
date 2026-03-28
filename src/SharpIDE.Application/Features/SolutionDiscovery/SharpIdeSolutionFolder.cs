using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.SolutionDiscovery;

public class SharpIdeSolutionFolder : ISharpIdeNode, IExpandableSharpIdeNode, IChildSharpIdeNode
{
	public required string Name { get; set; }
	public required string VsPersistencePath { get; set; }
	public required ObservableHashSet<SharpIdeSolutionFolder> Folders { get; set; }
	public required ObservableHashSet<SharpIdeProjectModel> Projects { get; set; }
	public required ObservableHashSet<SharpIdeFile> Files { get; set; }
	public bool Expanded { get; set; }
	public required IExpandableSharpIdeNode Parent { get; set; }

	[SetsRequiredMembers]
	internal SharpIdeSolutionFolder(IntermediateSlnFolderModel intermediateModel, ConcurrentBag<SharpIdeProjectModel> allProjects, ConcurrentBag<SharpIdeFile> allFiles, ConcurrentBag<SharpIdeFolder> allFolders, IExpandableSharpIdeNode parent)
	{
		Name = intermediateModel.Model.Name;
		VsPersistencePath = intermediateModel.Model.Path;
		Parent = parent;
		Files = new ObservableHashSet<SharpIdeFile>(intermediateModel.Files.Select(s => new SharpIdeFile(s.FullPath, s.Name, s.Extension, this, allFiles)));
		Folders = new ObservableHashSet<SharpIdeSolutionFolder>(intermediateModel.Folders.Select(x => new SharpIdeSolutionFolder(x, allProjects, allFiles, allFolders, this)));
		Projects = new ObservableHashSet<SharpIdeProjectModel>(intermediateModel.Projects.Select(x => new SharpIdeProjectModel(x, allProjects, allFiles, allFolders, this)));
	}
}
