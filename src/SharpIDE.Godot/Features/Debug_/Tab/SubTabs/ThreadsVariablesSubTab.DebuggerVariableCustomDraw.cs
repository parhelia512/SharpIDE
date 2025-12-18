using Godot;

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

        var font = _variablesTree.GetThemeFont(ThemeStringNames.Font);
        var fontSize = _variablesTree.GetThemeFontSize(ThemeStringNames.FontSize);
        const float padding = 4.0f;

        var currentX = rect.Position.X + padding;
        var textYPos = rect.Position.Y + (rect.Size.Y + fontSize) / 2 - 2;

        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), variable.Name, HorizontalAlignment.Left, -1,
            fontSize, VariableNameColor);
        var variableNameDrawnSize = font.GetStringSize(variable.Name, HorizontalAlignment.Left, -1, fontSize).X;
        currentX += variableNameDrawnSize;
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), " = ", HorizontalAlignment.Left, -1, fontSize,
            VariableWhiteColor);
        currentX += font.GetStringSize(" = ", HorizontalAlignment.Left, -1, fontSize).X;
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), $"{{{variable.Type}}} ",
            HorizontalAlignment.Left, -1, fontSize, VariableTypeColor);
        var variableTypeDrawnSize =
            font.GetStringSize($"{{{variable.Type}}} ", HorizontalAlignment.Left, -1, fontSize).X;
        currentX += variableTypeDrawnSize;
        _variablesTree.DrawString(font, new Vector2(currentX, textYPos), variable.Value, HorizontalAlignment.Left, -1,
            fontSize, VariableWhiteColor);
    }
}