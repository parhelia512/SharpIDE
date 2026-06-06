using Godot;

namespace SharpIDE.Godot.Features.ToolPanes;

[GlobalClass]
public partial class ToolPaneResource : Resource
{
	[Export]
	public required PackedScene Pane { get; set; }

	[Export]
	public required DpiTexture ButtonIcon { get; set; }

	[Export]
	public required string ButtonLabel { get; set; }

	[Export]
	public ToolPaneType ToolPaneType { get; set; }

	public Control? PaneInstance { get; set; }
}
