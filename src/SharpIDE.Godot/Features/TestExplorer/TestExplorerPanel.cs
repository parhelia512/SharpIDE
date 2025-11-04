using Godot;
using SharpIDE.Application.Features.Build;
using SharpIDE.Application.Features.Testing;

namespace SharpIDE.Godot.Features.TestExplorer;

public partial class TestExplorerPanel : Control
{
    [Inject] private readonly SharpIdeSolutionAccessor _solutionAccessor = null!;
    [Inject] private readonly TestRunnerService _testRunnerService = null!;
    [Inject] private readonly BuildService _buildService = null!;
    
    private readonly PackedScene _testNodeEntryScene = ResourceLoader.Load<PackedScene>("uid://dt50f2of66dlt");

    private Button _refreshButton = null!;
    private VBoxContainer _testNodesVBoxContainer = null!;

    public override void _Ready()
    {
        _refreshButton = GetNode<Button>("%RefreshButton");
        _testNodesVBoxContainer = GetNode<VBoxContainer>("%TestNodesVBoxContainer");
        _ = Task.GodotRun(AsyncReady);
        _refreshButton.Pressed += OnRefreshButtonPressed;
    }

    private async Task AsyncReady()
    {
        await DiscoverTestNodesForSolution(false);
    }
    
    private void OnRefreshButtonPressed()
    {
        _ = Task.GodotRun(() => DiscoverTestNodesForSolution(true));
    }

    private async Task DiscoverTestNodesForSolution(bool withBuild)
    {
        await _solutionAccessor.SolutionReadyTcs.Task;
        var solution = _solutionAccessor.SolutionModel!;
        if (withBuild)
        {
            await _buildService.MsBuildAsync(solution.FilePath);
        }
        var testNodes = await _testRunnerService.DiscoverTests(solution);
        testNodes.ForEach(s => GD.Print(s.DisplayName));
        var scenes = testNodes.Select(s =>
        {
            var entry = _testNodeEntryScene.Instantiate<TestNodeEntry>();
            entry.TestNode = s;
            return entry;
        });
        await this.InvokeAsync(() =>
        {
            _testNodesVBoxContainer.QueueFreeChildren();
            foreach (var scene in scenes)
            {
                _testNodesVBoxContainer.AddChild(scene);
            }
        });
    }
}