using Godot;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Godot.Features.SolutionExplorer;

file enum ProjectContextMenuOptions
{
    Run = 0,
    Build = 1,
    Rebuild = 2,
    Clean = 3,
    Restore = 4
}

public partial class SolutionExplorerPanel
{
    private void OpenContextMenuProject(SharpIdeProjectModel project)
    {
        var menu = new PopupMenu();
        AddChild(menu);
        menu.AddItem("Run", (int)ProjectContextMenuOptions.Run);
        menu.AddSeparator();
        menu.AddItem("Build", (int)ProjectContextMenuOptions.Build);
        menu.AddItem("Rebuild", (int)ProjectContextMenuOptions.Rebuild);
        menu.AddItem("Clean", (int)ProjectContextMenuOptions.Clean);
        menu.AddItem("Restore", (int)ProjectContextMenuOptions.Restore);
        menu.PopupHide += () => menu.QueueFree();
        menu.IdPressed += id =>
        {
            var actionId = (ProjectContextMenuOptions)id;
            if (actionId is ProjectContextMenuOptions.Run)
            {
                
            }
            if (actionId is ProjectContextMenuOptions.Build)
            {
                
            }
            else if (actionId is ProjectContextMenuOptions.Rebuild)
            {
                
            }
            else if (actionId is ProjectContextMenuOptions.Clean)
            {
                
            }
            else if (actionId is ProjectContextMenuOptions.Restore)
            {
                
            }
        };
			
        var globalMousePosition = GetGlobalMousePosition();
        menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
        menu.Popup();
    }
}