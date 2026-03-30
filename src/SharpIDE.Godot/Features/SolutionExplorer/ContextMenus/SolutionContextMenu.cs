using Godot;
using SharpIDE.Application.Features.SolutionDiscovery;

namespace SharpIDE.Godot.Features.SolutionExplorer;

file enum SolutionContextMenuOptions
{
    Add = 1,
    ReloadSolution
}

file enum SolutionAddSubmenuOptions
{
    SolutionFolder = 1
}

public partial class SolutionExplorerPanel
{
    private void OpenContextMenuSolution(SharpIdeSolutionModel solution, TreeItem solutionTreeItem)
    {
        var menu = new PopupMenu();
        AddChild(menu);
        
        var createNewSubmenu = new PopupMenu();
        menu.AddSubmenuNodeItem("Add", createNewSubmenu, (int)SolutionContextMenuOptions.Add);
        menu.SetItemDisabled(menu.GetItemIndex((int)SolutionContextMenuOptions.Add), true);
        createNewSubmenu.AddItem("New Solution Folder", (int)SolutionAddSubmenuOptions.SolutionFolder);
        createNewSubmenu.IdPressed += id => OnSolutionAddSubmenuPressed(id, solution);
        
        menu.PopupHide += () => menu.QueueFree();
        menu.IdPressed += id =>
        {
            var actionId = (SolutionContextMenuOptions)id;
        };
			
        var globalMousePosition = GetGlobalMousePosition();
        menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
        menu.Popup();
    }

    private void OnSolutionAddSubmenuPressed(long id, SharpIdeSolutionModel solution)
    {
        var actionId = (SolutionAddSubmenuOptions)id;
    }
}