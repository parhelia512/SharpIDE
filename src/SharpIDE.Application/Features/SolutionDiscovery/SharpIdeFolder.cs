using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
using R3;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Application.Features.SolutionDiscovery;

public class SharpIdeFolder : ISharpIdeNode, IExpandableSharpIdeNode, IChildSharpIdeNode, IFileOrFolder
{
	public required IExpandableSharpIdeNode Parent { get; set; }
	public required string Path { get; set; }
	public string ChildNodeBasePath => Path;
	public required ReactiveProperty<string> Name { get; set; }
	public required bool IsCsprojRootFolder { get; set; }
	public ObservableList<SharpIdeFile> Files { get; set; } = null!;
	public ObservableList<SharpIdeFolder> Folders { get; set; } = null!;
	public bool Expanded { get; set; }

	[SetsRequiredMembers]
	public SharpIdeFolder(DirectoryInfo folderInfo, IExpandableSharpIdeNode parent, ConcurrentBag<SharpIdeFile> allFiles, ConcurrentBag<SharpIdeFolder> allFolders)
	{
		Parent = parent;
		Path = folderInfo.FullName;
		Name = new ReactiveProperty<string>(folderInfo.Name);
		Files = new ObservableList<SharpIdeFile>(folderInfo.GetFiles(this, allFiles));
		Folders = new ObservableList<SharpIdeFolder>(this.GetSubFolders(this, allFiles, allFolders));
		IsCsprojRootFolder = Files.Any(s => s.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase));
	}

	public SharpIdeFolder()
	{

	}
}
