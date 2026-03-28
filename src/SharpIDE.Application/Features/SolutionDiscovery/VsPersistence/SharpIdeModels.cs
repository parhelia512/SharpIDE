using ObservableCollections;
using R3;

namespace SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

public interface ISharpIdeNode;

public interface IExpandableSharpIdeNode
{
	public bool Expanded { get; set; }
}
public interface ISolutionOrProject
{
	public string DirectoryPath { get; set; }
}
public interface IFolderOrProject : IExpandableSharpIdeNode, IChildSharpIdeNode
{
	public ObservableList<SharpIdeFolder> Folders { get; init; }
	public ObservableList<SharpIdeFile> Files { get; init; }
	public ReactiveProperty<string> Name { get; set; }
	public string ChildNodeBasePath { get; }
}
public interface IFileOrFolder : IChildSharpIdeNode
{
	public string Path { get; set; }
	public ReactiveProperty<string> Name { get; set; }
}
public interface IChildSharpIdeNode
{
	public IExpandableSharpIdeNode Parent { get; set; }

	// TODO: Profile/redesign
	public SharpIdeProjectModel? GetNearestProjectNode()
	{
		var current = this;
		while (current is not SharpIdeProjectModel && current?.Parent is not null)
		{
			current = current.Parent as IChildSharpIdeNode;
		}
		return current as SharpIdeProjectModel;
	}
}
