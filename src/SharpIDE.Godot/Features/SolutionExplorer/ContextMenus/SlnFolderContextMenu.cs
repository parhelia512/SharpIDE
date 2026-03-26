using Godot;
using SharpIDE.Application.Features.FileWatching;
using SharpIDE.Application.Features.SolutionDiscovery;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;
using SharpIDE.Godot.Features.SolutionExplorer.ContextMenus.Dialogs;

namespace SharpIDE.Godot.Features.SolutionExplorer;

file enum SlnFolderContextMenuOptions
{
    Add = 1
}

file enum SlnFolderAddSubmenuOptions
{
    Project = 1
}

public partial class SolutionExplorerPanel
{
    private PackedScene _newProjectContainerScene = ResourceLoader.Load<PackedScene>("uid://vkqiqm661f");
    
    private void OpenContextMenuSlnFolder(SharpIdeSolutionFolder slnFolder, TreeItem slnFolderTreeItem)
    {
        var menu = new PopupMenu();
        AddChild(menu);
        
        var createNewSubmenu = new PopupMenu();
        menu.AddSubmenuNodeItem("Add", createNewSubmenu, (int)SlnFolderContextMenuOptions.Add);
        createNewSubmenu.AddItem("New Project", (int)SlnFolderAddSubmenuOptions.Project);
        createNewSubmenu.SetItemDisabled(createNewSubmenu.GetItemIndex((int)SlnFolderAddSubmenuOptions.Project), true);
        createNewSubmenu.IdPressed += id => OnSlnFolderAddSubmenuPressed(id, slnFolder);
        
        menu.PopupHide += () => menu.QueueFree();
        menu.IdPressed += id =>
        {
            var actionId = (SlnFolderContextMenuOptions)id;
            // if (actionId is SlnFolderContextMenuOptions.Add)
            // {
            //     
            // }
        };
			
        var globalMousePosition = GetGlobalMousePosition();
        menu.Position = new Vector2I((int)globalMousePosition.X, (int)globalMousePosition.Y);
        menu.Popup();
    }

    private void OnSlnFolderAddSubmenuPressed(long id, SharpIdeSolutionFolder slnFolder)
    {
        var actionId = (SlnFolderAddSubmenuOptions)id;
        if (actionId is SlnFolderAddSubmenuOptions.Project)
        {
            var newProjectContainer = _newProjectContainerScene.Instantiate<NewProject.NewProjectContainer>();
            var popupWindow = new Window
            {
                Title = "New Project",
                InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
                Transient = true,
                PopupWindow = true,
                PopupWMHint = true
            };
            popupWindow.CloseRequested += () => this.RemoveChildAndQueueFree(popupWindow);
            popupWindow.AddChild(newProjectContainer);
            AddChild(popupWindow);
            popupWindow.PopupCenteredRatio(0.5f);
        }
    }
}