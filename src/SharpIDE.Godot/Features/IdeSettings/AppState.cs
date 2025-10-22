namespace SharpIDE.Godot.Features.IdeSettings;

public class AppState
{
    public string? LastOpenSolutionFilePath { get; set; }
    public IdeSettings IdeSettings { get; set; } = new IdeSettings();
    public List<RecentSln> RecentSlns { get; set; } = [];
}

public class IdeSettings
{
    public bool AutoOpenLastSolution { get; set; }
}

public record RecentSln
{
    public required string Name { get; set; }
    public required string FilePath { get; set; }
    public IdeSolutionState IdeSolutionState { get; set; } = new IdeSolutionState();
}