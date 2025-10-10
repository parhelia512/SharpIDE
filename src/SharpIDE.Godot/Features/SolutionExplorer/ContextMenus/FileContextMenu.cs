using Godot;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Godot.Features.SolutionExplorer;

file enum FileContextMenuOptions
{
    Open = 0,
    RevealInFileExplorer = 1,
    CopyFullPath = 2
}

public partial class SolutionExplorerPanel
{
    private void OpenContextMenuFile(SharpIdeFile file)
    {
        var menu = new PopupMenu();
        AddChild(menu);
        menu.AddItem("Open", (int)FileContextMenuOptions.Open);
        menu.AddItem("Reveal in File Explorer", (int)FileContextMenuOptions.RevealInFileExplorer);
        menu.AddSeparator();
        menu.AddItem("Copy Full Path", (int)FileContextMenuOptions.CopyFullPath);
        menu.PopupHide += () => menu.QueueFree();
        menu.IdPressed += id =>
        {
            var actionId = (FileContextMenuOptions)id;
            if (actionId is FileContextMenuOptions.Open)
            {
                GodotGlobalEvents.Instance.FileSelected.InvokeParallelFireAndForget(file, null);
            }
            else if (actionId is FileContextMenuOptions.RevealInFileExplorer)
            {
                OS.ShellOpen(Path.GetDirectoryName(file.Path)!);
            }
            else if (actionId is FileContextMenuOptions.CopyFullPath)
            {
                DisplayServer.ClipboardSet(file.Path);
            }
        };
			
        var globalMousePosition = GetGlobalMousePosition();
        menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
        menu.Popup();
    }
}