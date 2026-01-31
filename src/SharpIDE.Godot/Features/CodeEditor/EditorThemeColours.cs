using Godot;

namespace SharpIDE.Godot;

public static class EditorThemeColours
{
    public static readonly EditorThemeColorSet Light = new EditorThemeColorSet
    {
        Orange = CachedColorsLight.Orange,
        White = CachedColorsLight.White,
        Yellow = CachedColorsLight.Yellow,
        CommentGreen = CachedColorsLight.CommentGreen,
        KeywordBlue = CachedColorsLight.KeywordBlue,
        LightOrangeBrown = CachedColorsLight.LightOrangeBrown,
        NumberGreen = CachedColorsLight.NumberGreen,
        InterfaceGreen = CachedColorsLight.InterfaceGreen,
        ClassGreen = CachedColorsLight.ClassGreen,
        VariableBlue = CachedColorsLight.VariableBlue,
        Gray = CachedColorsLight.Gray,
        Pink = CachedColorsLight.Pink,
        ErrorRed = CachedColorsLight.ErrorRed,
        
        RazorComponentGreen = CachedColorsLight.RazorComponentGreen,
        RazorMetaCodePurple = CachedColorsLight.RazorMetaCodePurple,
        HtmlDelimiterGray = CachedColorsLight.HtmlDelimiterGray
    };
    
    public static readonly EditorThemeColorSet Dark = new EditorThemeColorSet
    {
        Orange = CachedColors.Orange,
        White = CachedColors.White,
        Yellow = CachedColors.Yellow,
        CommentGreen = CachedColors.CommentGreen,
        KeywordBlue = CachedColors.KeywordBlue,
        LightOrangeBrown = CachedColors.LightOrangeBrown,
        NumberGreen = CachedColors.NumberGreen,
        InterfaceGreen = CachedColors.InterfaceGreen,
        ClassGreen = CachedColors.ClassGreen,
        VariableBlue = CachedColors.VariableBlue,
        Gray = CachedColors.Gray,
        Pink = CachedColors.Pink,
        ErrorRed = CachedColors.ErrorRed,
        
        RazorComponentGreen = CachedColors.RazorComponentGreen,
        RazorMetaCodePurple = CachedColors.RazorMetaCodePurple,
        HtmlDelimiterGray = CachedColors.HtmlDelimiterGray
    };
}

public class EditorThemeColorSet
{
    public required Color Orange;
    public required Color White;
    public required Color Yellow;
    public required Color CommentGreen;
    public required Color KeywordBlue;
    public required Color LightOrangeBrown;
    public required Color NumberGreen;
    public required Color InterfaceGreen;
    public required Color ClassGreen;
    public required Color VariableBlue;
    public required Color Gray;
    public required Color Pink;
    public required Color ErrorRed;
    
    public required Color RazorComponentGreen;
    public required Color RazorMetaCodePurple;
    public required Color HtmlDelimiterGray;
}