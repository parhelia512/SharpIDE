using Godot;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace SharpIDE.Godot.Features.Debug_.Tab.SubTabs;

public partial class ThreadsVariablesSubTab
{
    private static readonly Color VariableNameColor = new Color("f0ac81");
    private static readonly Color VariableWhiteColor = new Color("d4d4d4");
    private static readonly Color VariableTypeColor = new Color("70737a");
    
    private void DebuggerVariableCustomDraw(TreeItem treeItem, Rect2 rect)
    {
        var variable = _variableReferenceLookup.GetValueOrDefault(treeItem);
        if (variable is null) return;
        
        const int iconSize = 18;
        var icon = variable.PresentationHint?.Kind switch
        {
            VariablePresentationHint.KindValue.Data => _fieldIcon,
            VariablePresentationHint.KindValue.Property => _propertyIcon,
            VariablePresentationHint.KindValue.Class => _staticMembersIcon,
            _ => null
        };
        if (icon is null)
        {
            // unlike sharpdbg and presumably vsdbg, netcoredbg does not set PresentationHint for variables
            if (variable.Name == "Static members") icon = _staticMembersIcon; // Will not currently occur, as 'Static members' are not handled by this custom draw
            else icon = _fieldIcon;
        }

        var font = _variablesTree.GetThemeFont(ThemeStringNames.Font);
        var fontSize = _variablesTree.GetThemeFontSize(ThemeStringNames.FontSize);
        const float padding = 4.0f;

        var currentX = rect.Position.X + padding;
        var textYPos = rect.Position.Y + (rect.Size.Y + fontSize) / 2 - 2;

        var iconRect = new Rect2(currentX, rect.Position.Y + (rect.Size.Y - iconSize) / 2, iconSize, iconSize);
        _variablesTree.DrawTextureRect(icon, iconRect, false);
        currentX += iconSize + padding;

        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), variable.Name, HorizontalAlignment.Left, -1, fontSize, VariableNameColor);
        var variableNameDrawnWidth = font.GetStringSize(variable.Name, HorizontalAlignment.Left, -1, fontSize).X;
        currentX += variableNameDrawnWidth + padding;
        const string equalsString = "=";
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), equalsString, HorizontalAlignment.Left, -1, fontSize, VariableWhiteColor);
        var equalsWidth = font.GetStringSize(equalsString, HorizontalAlignment.Left, -1, fontSize).X;
        currentX += equalsWidth + padding;
        var variableTypeDisplayString = $$"""{{{variable.Type}}}""";
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), variableTypeDisplayString, HorizontalAlignment.Left, -1, fontSize, VariableTypeColor);
        var variableTypeDrawnSize = font.GetStringSize(variableTypeDisplayString, HorizontalAlignment.Left, -1, fontSize).X;
        currentX += variableTypeDrawnSize + padding;
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), variable.Value, HorizontalAlignment.Left, -1, fontSize, VariableWhiteColor);
    }
}