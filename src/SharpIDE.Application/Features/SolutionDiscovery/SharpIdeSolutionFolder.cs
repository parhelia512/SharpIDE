using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
using SharpIDE.Application.Features.FileSystem;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.SolutionDiscovery;

public class SharpIdeSolutionFolder : ISharpIdeNode, IExpandableSharpIdeNode, IChildSharpIdeNode
{
	public required string Name { get; set; }
	public required string VsPersistencePath { get; set; }
	public required ObservableHashSet<SharpIdeSolutionFolder> Folders { get; set; }
	public required ObservableHashSet<SharpIdeProjectModel> Projects { get; set; }
	public required ObservableHashSet<SharpIdeSolutionFile> Files { get; set; }
	public bool Expanded { get; set; }
	public required IExpandableSharpIdeNode Parent { get; set; }

	[SetsRequiredMembers]
	internal SharpIdeSolutionFolder(IntermediateSlnFolderModel intermediateModel, ConcurrentBag<SharpIdeProjectModel> allProjects, ConcurrentBag<SharpIdeFile> allFiles, ConcurrentBag<SharpIdeFolder> allFolders, IExpandableSharpIdeNode parent, SharpIdeRootFolder sharpIdeRootFolder)
	{
		Name = intermediateModel.Model.Name;
		VsPersistencePath = intermediateModel.Model.Path;
		Parent = parent;
		Files = new ObservableHashSet<SharpIdeSolutionFile>(intermediateModel.Files.Select(s => new SharpIdeSolutionFile(s.FullPath, s.Name, s.Extension, this, sharpIdeRootFolder)));
		Folders = new ObservableHashSet<SharpIdeSolutionFolder>(intermediateModel.Folders.Select(x => new SharpIdeSolutionFolder(x, allProjects, allFiles, allFolders, this, sharpIdeRootFolder)));
		Projects = new ObservableHashSet<SharpIdeProjectModel>(intermediateModel.Projects.Select(x => new SharpIdeProjectModel(x, allProjects, allFiles, allFolders, this, sharpIdeRootFolder)));
	}
}
