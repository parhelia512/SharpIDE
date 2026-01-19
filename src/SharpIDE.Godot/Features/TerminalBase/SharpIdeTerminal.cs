using GDExtensionBindgen;
using Godot;

namespace SharpIDE.Godot.Features.TerminalBase;

public partial class SharpIdeTerminal : Control
{
	private Terminal _terminal = null!;
	public override void _Ready()
	{
		var terminalControl = GetNode<Control>("Terminal");
		_terminal = new Terminal(terminalControl);
	}
	
	[RequiresGodotUiThread]
	public void Write(string text)
	{
		_terminal.Write(text);
	}
	
	public async Task WriteAsync(byte[] text)
	{
		await this.InvokeAsync(() => _terminal.Write(text));
	}
	
	[RequiresGodotUiThread]
	public void ClearTerminal()
	{
		// .Clear removes all text except for the bottom row, so lets make sure we have a blank line, and cursor at start
		_terminal.Write("\r\n");
		_terminal.Clear();
	}
}