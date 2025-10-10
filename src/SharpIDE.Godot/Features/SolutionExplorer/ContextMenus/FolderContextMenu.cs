using Godot;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Godot.Features.SolutionExplorer;

file enum FolderContextMenuOptions
{
    RevealInFileExplorer = 1
}

public partial class SolutionExplorerPanel
{
    private void OpenContextMenuFolder(SharpIdeFolder folder)
    {
        var menu = new PopupMenu();
        AddChild(menu);
        menu.AddItem("Reveal in File Explorer", (int)FolderContextMenuOptions.RevealInFileExplorer);
        menu.PopupHide += () => menu.QueueFree();
        menu.IdPressed += id =>
        {
            var actionId = (FolderContextMenuOptions)id;
            if (actionId is FolderContextMenuOptions.RevealInFileExplorer)
            {
                OS.ShellOpen(folder.Path);
            }
        };
			
        var globalMousePosition = GetGlobalMousePosition();
        menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
        menu.Popup();
    }
}