using Godot;
using Microsoft.CodeAnalysis.Text;

namespace SharpIDE.Godot.Features.CodeEditor;

public partial class SharpIdeCodeEdit
{
    private bool _isMethodSignatureHelpPopupOpen;
    public bool MethodSignatureHelpPopupTryConsumeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton) return false;
        if (@event.IsActionPressed(InputStringNames.Cancel) && _isMethodSignatureHelpPopupOpen)
        {
            _isMethodSignatureHelpPopupOpen = false;
            _methodSignatureHelpWindow.Hide();
            return true;
        }
        else if (@event.IsActionPressed(InputStringNames.CodeEditorRequestSignatureInfo))
        {
            var (caretLine, caretColumn) = GetCaretPosition();
            var caretPos = GetPosAtLineColumn(caretLine, caretColumn);
            _ = Task.GodotRun(async () =>
            {
                var linePos = new LinePosition(caretLine, caretColumn);
                var signatureHelpItems = await _roslynAnalysis.GetMethodSignatureInfo(_currentFile, linePos);
                if (signatureHelpItems != null)
                {
                    await this.InvokeAsync(() =>
                    {
                        var richTextLabel = _methodSignatureHelpWindow.GetNode<RichTextLabel>("PanelContainer/RichTextLabel");
                        richTextLabel.Clear();
                        MethodSignatureHelpTooltip.WriteToMethodSignatureHelpLabel(richTextLabel, signatureHelpItems, _syntaxHighlighter.ColourSetForTheme);
                        _methodSignatureHelpWindow.Position = (Vector2I)GetGlobalPosition() + caretPos;
                        _methodSignatureHelpWindow.Show();
                        _isMethodSignatureHelpPopupOpen = true;
                    });
                }
            });
            return true;
        }
        return false;
    }
}