using Godot;
using Microsoft.CodeAnalysis.Text;

namespace SharpIDE.Godot.Features.CodeEditor;

public partial class SharpIdeCodeEdit
{
    private bool _isMethodSignatureHelpPopupOpen;
    private LinePositionSpan _signatureHelpApplicableSpan;
    
    private bool MethodSignatureHelpPopupTryConsumeGuiInput(InputEvent @event)
    {
        if (_isMethodSignatureHelpPopupOpen)
        {
            var caretPos = GetCaretPosition();
            var caretPositionLinePosition = new LinePosition(caretPos.line, caretPos.col);
            if (caretPositionLinePosition < _signatureHelpApplicableSpan.Start || caretPositionLinePosition > _signatureHelpApplicableSpan.End)
            {
                _isMethodSignatureHelpPopupOpen = false;
                _methodSignatureHelpWindow.Hide();
                return false;
            }
        }
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
                var sharpIdeSignatureHelpItems = await _roslynAnalysis.GetMethodSignatureInfo(_currentFile, linePos);
                if (sharpIdeSignatureHelpItems is null) return;
                var signatureHelpItems = sharpIdeSignatureHelpItems.Items;
                _signatureHelpApplicableSpan = sharpIdeSignatureHelpItems.ApplicableSpan;
                await this.InvokeAsync(() =>
                {
                    var richTextLabel = _methodSignatureHelpWindow.GetNode<RichTextLabel>("PanelContainer/RichTextLabel");
                    richTextLabel.Clear();
                    _methodSignatureHelpWindow.Size = new Vector2I(10, 10); // Used to shrink the window, as ChildControlsChanged() doesn't seem to handle shrinking in this case?
                    MethodSignatureHelpTooltip.WriteToMethodSignatureHelpLabel(richTextLabel, signatureHelpItems, _syntaxHighlighter.ColourSetForTheme);
                    _methodSignatureHelpWindow.Position = (Vector2I)GetGlobalPosition() + caretPos;
                    _methodSignatureHelpWindow.Show();
                    _isMethodSignatureHelpPopupOpen = true;
                });
            });
            return true;
        }
        return false;
    }
}