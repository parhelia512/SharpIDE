using System.Collections.Specialized;
using Godot;
using ObservableCollections;
using R3;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Godot.Features.Problems;

public partial class ProblemsPanel : Control
{
    private PackedScene _problemsPanelProjectEntryScene = GD.Load<PackedScene>("uid://do72lghjvfdp3");
    private VBoxContainer _vBoxContainer = null!;
    // TODO: Use observable collections in the solution model and downwards
    public SharpIdeSolutionModel? Solution { get; set; }
    private readonly ObservableHashSet<SharpIdeProjectModel> _projects = [];

    public override void _Ready()
    {
        Observable.EveryValueChanged(this, manager => manager.Solution)
            .Where(s => s is not null)
            .Subscribe(s =>
            {
                GD.Print($"ProblemsPanel: Solution changed to {s?.Name ?? "null"}");
                _projects.Clear();
                _projects.AddRange(s!.AllProjects);
            });
        _vBoxContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
        _vBoxContainer.GetChildren().ToList().ForEach(c => c.QueueFree());
        _vBoxContainer.BindChildren(_projects, _problemsPanelProjectEntryScene);
    }
}