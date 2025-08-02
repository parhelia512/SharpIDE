namespace SharpIDE.Photino.Models;

public class AppState
{
	public required string SolutionFilePath { get; set; }
	public required IdeSettings IdeSettings { get; set; } = new IdeSettings();
}

public class IdeSettings
{
	public bool AutoOpenLastSolution { get; set; }
}
