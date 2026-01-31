using System.Collections.Immutable;
using Godot;
using Godot.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using SharpIDE.Application.Features.Analysis;
using SharpIDE.Application.Features.Analysis.Razor;
using SharpIDE.Godot.Features.CodeEditor;
using SharpIDE.Godot.Features.IdeSettings;

namespace SharpIDE.Godot;

public partial class CustomHighlighter : SyntaxHighlighter
{
    private readonly Dictionary _emptyDict = new();

    private System.Collections.Generic.Dictionary<int, ImmutableArray<SharpIdeRazorClassifiedSpan>> _razorClassifiedSpansByLine = [];
    private System.Collections.Generic.Dictionary<int, ImmutableArray<SharpIdeClassifiedSpan>> _classifiedSpansByLine = [];

    private EditorThemeColorSet _colourSetForTheme = null!;
    
    public void UpdateThemeColorCache(LightOrDarkTheme themeType)
    {
        _colourSetForTheme = themeType switch
        {
            LightOrDarkTheme.Light => EditorThemeColours.Light,
            LightOrDarkTheme.Dark => EditorThemeColours.Dark,
            _ => throw new NotImplementedException("Unknown theme type")
        };
    }
    
    
    public void SetHighlightingData(ImmutableArray<SharpIdeClassifiedSpan> classifiedSpans, ImmutableArray<SharpIdeRazorClassifiedSpan> razorClassifiedSpans)
    {
        // separate each line here
        var razorSpansForLine = razorClassifiedSpans
            .Where(s => s.Span.Length is not 0)
            .GroupBy(s => s.Span.LineIndex);
        
        _razorClassifiedSpansByLine = razorSpansForLine.ToDictionary(g => g.Key, g => g.ToImmutableArray());

        var spansGroupedByFileSpan = classifiedSpans
            .Where(s => s.ClassifiedSpan.TextSpan.Length is not 0)
            .GroupBy(span => span.FileSpan.Start.Line);
        
        _classifiedSpansByLine = spansGroupedByFileSpan.ToDictionary(g => g.Key, g => g.ToImmutableArray());
    }

    // Indicates that lines were removed or added, and the overall result of that is that a line (wasLineNumber), is now (becameLineNumber)
    // So if you added a line above line 10, then wasLineNumber=10, becameLineNumber=11
    // If you removed a line above line 10, then wasLineNumber=10, becameLineNumber=9
    //
    // This is all a very dodgy workaround to move highlighting up and down, while we wait for the workspace to return us highlighting for the updated file
    public void LinesChanged(long wasLineNumber, long becameLineNumber, SharpIdeCodeEdit.LineEditOrigin origin)
    {
        var difference = (int)(becameLineNumber - wasLineNumber);
        if (difference is 0) return;
        if (difference > 0)
        {
            LinesAdded(wasLineNumber, difference, origin);
        }
        else
        {
            LinesRemoved(wasLineNumber, -difference);
        }
    }

    private void LinesAdded(long fromLine, int difference, SharpIdeCodeEdit.LineEditOrigin origin)
    {
        _razorClassifiedSpansByLine = Rearrange(_razorClassifiedSpansByLine, fromLine, difference, origin);
        _classifiedSpansByLine = Rearrange(_classifiedSpansByLine, fromLine, difference, origin);
        return;

        static System.Collections.Generic.Dictionary<int, T> Rearrange<T>(System.Collections.Generic.Dictionary<int, T> existingDictionary, long fromLine, int difference, SharpIdeCodeEdit.LineEditOrigin origin)
        {
            var newDict = new System.Collections.Generic.Dictionary<int, T>();
            foreach (var kvp in existingDictionary)
            {
                bool shouldShift =
                    kvp.Key > fromLine ||                // always shift lines after the insertion point
                    (origin == SharpIdeCodeEdit.LineEditOrigin.StartOfLine && kvp.Key == fromLine); // shift current line if origin is Start

                int newKey = shouldShift ? kvp.Key + difference : kvp.Key;
                newDict[newKey] = kvp.Value;
            }
            return newDict;
        }
    }
    
    private void LinesRemoved(long fromLine, int numberOfLinesRemoved)
    {
        _classifiedSpansByLine = Rearrange(_classifiedSpansByLine, fromLine, numberOfLinesRemoved);
        _razorClassifiedSpansByLine = Rearrange(_razorClassifiedSpansByLine, fromLine, numberOfLinesRemoved);
        return;

        static System.Collections.Generic.Dictionary<int, T> Rearrange<T>(System.Collections.Generic.Dictionary<int, T> existingDictionary, long fromLine, int numberOfLinesRemoved)
        {
            // everything from 'fromLine' onwards needs to be shifted up by numberOfLinesRemoved
            var newDict = new System.Collections.Generic.Dictionary<int, T>();
            foreach (var kvp in existingDictionary)
            {
                if (kvp.Key < fromLine)
                {
                    newDict[kvp.Key] = kvp.Value;
                }
                else if (kvp.Key == fromLine)
                {
                    newDict[kvp.Key - numberOfLinesRemoved] = kvp.Value;
                }
                else if (kvp.Key >= fromLine + numberOfLinesRemoved)
                {
                    newDict[kvp.Key - numberOfLinesRemoved] = kvp.Value;
                } 
            }
            return newDict;
        }
    }
    
    public override Dictionary _GetLineSyntaxHighlighting(int line)
    {
        var highlights = (_classifiedSpansByLine, _razorClassifiedSpansByLine) switch
        {
            ({ Count: 0 }, { Count: 0 }) => _emptyDict,
            ({ Count: > 0 }, _) => MapClassifiedSpansToHighlights(line),
            (_, { Count: > 0 }) => MapRazorClassifiedSpansToHighlights(line),
            _ => throw new NotImplementedException("Both ClassifiedSpans and RazorClassifiedSpans are set. This is not supported yet.")
        };

        return highlights;
    }
    
    private static readonly StringName ColorStringName = "color";
    private Dictionary MapRazorClassifiedSpansToHighlights(int line)
    {
        var highlights = new Dictionary();
        if (_razorClassifiedSpansByLine.TryGetValue(line, out var razorSpansForLine) is false) return highlights;
        
        // group by span (start, length matches)
        var spansGroupedByFileSpan = razorSpansForLine.GroupBy(span => span.Span);

        foreach (var razorSpanGrouping in spansGroupedByFileSpan)
        {
            var spans = razorSpanGrouping.ToList();
            if (spans.Count > 2) throw new NotImplementedException("More than 2 classified spans is not supported yet.");
            if (spans.Count is not 1)
            {
                if (spans.Any(s => s.Kind is SharpIdeRazorSpanKind.Code))
                {
                    spans = spans.Where(s => s.Kind is SharpIdeRazorSpanKind.Code).ToList();
                }
                if (spans.Count is not 1)
                {
                    SharpIdeRazorClassifiedSpan? staticClassifiedSpan = spans.FirstOrDefault(s => s.CodeClassificationType == ClassificationTypeNames.StaticSymbol);
                    if (staticClassifiedSpan is not null) spans.Remove(staticClassifiedSpan.Value);
                }
            }
            var razorSpan = spans.Single();
            
            int columnIndex = razorSpan.Span.CharacterIndex;
            
            var highlightInfo = new Dictionary
            {
                { ColorStringName, GetColorForRazorSpanKind(razorSpan.Kind, razorSpan.CodeClassificationType, razorSpan.VsSemanticRangeType) }
            };

            highlights[columnIndex] = highlightInfo;
        }

        return highlights;
    }
    
    private Color GetColorForRazorSpanKind(SharpIdeRazorSpanKind kind, string? codeClassificationType, string? vsSemanticRangeType)
    {
        return kind switch
        {
            SharpIdeRazorSpanKind.Code => GetColorForClassification(codeClassificationType!),
            SharpIdeRazorSpanKind.Comment => _colourSetForTheme.CommentGreen, // green
            SharpIdeRazorSpanKind.MetaCode => _colourSetForTheme.RazorMetaCodePurple, // purple
            SharpIdeRazorSpanKind.Markup => GetColorForMarkupSpanKind(vsSemanticRangeType),
            SharpIdeRazorSpanKind.Transition => _colourSetForTheme.RazorMetaCodePurple, // purple
            SharpIdeRazorSpanKind.None => _colourSetForTheme.White,
            _ => _colourSetForTheme.White
        };
    }
    
    private Color GetColorForMarkupSpanKind(string? vsSemanticRangeType)
    {
        return vsSemanticRangeType switch
        {
            "razorDirective" or "razorTransition" => _colourSetForTheme.RazorMetaCodePurple, // purple
            "markupTagDelimiter" => _colourSetForTheme.HtmlDelimiterGray, // gray
            "markupTextLiteral" => _colourSetForTheme.White, // white
            "markupElement" => _colourSetForTheme.KeywordBlue, // blue
            "razorComponentElement" => _colourSetForTheme.RazorComponentGreen, // dark green
            "razorComponentAttribute" => _colourSetForTheme.White, // white
            "razorComment" or "razorCommentStar" or "razorCommentTransition" => _colourSetForTheme.CommentGreen, // green
            "markupOperator" => _colourSetForTheme.White, // white
            "markupAttributeQuote" => _colourSetForTheme.White, // white
            _ => _colourSetForTheme.White // default to white
        };
    }

    
    private Dictionary MapClassifiedSpansToHighlights(int line)
    {
        var highlights = new Dictionary();
        if (_classifiedSpansByLine.TryGetValue(line, out var spansForLine) is false) return highlights;
        
        // consider no linq or ZLinq
        // group by span (start, length matches)
        var spansGroupedByFileSpan = spansForLine
            .GroupBy(span => span.FileSpan)
            .Select(group => (fileSpan: group.Key, classifiedSpans: group.Select(s => s.ClassifiedSpan).ToList()));

        foreach (var (fileSpan, classifiedSpans) in spansGroupedByFileSpan)
        {
            if (classifiedSpans.Count > 2) throw new NotImplementedException("More than 2 classified spans is not supported yet.");
            if (classifiedSpans.Count is not 1)
            {
                ClassifiedSpan? staticClassifiedSpan = classifiedSpans.FirstOrDefault(s => s.ClassificationType == ClassificationTypeNames.StaticSymbol);
                if (staticClassifiedSpan is not null) classifiedSpans.Remove(staticClassifiedSpan.Value);
            }
            // Column index of the first character in this span
            int columnIndex = fileSpan.Start.Character;

            // Build the highlight entry
            var highlightInfo = new Dictionary
            {
                { ColorStringName, GetColorForClassification(classifiedSpans.Single().ClassificationType) }
            };

            highlights[columnIndex] = highlightInfo;
        }

        return highlights;
    }
    
    private Color GetColorForClassification(string classificationType)
    {
        var colour = classificationType switch
        {
            // Keywords
            "keyword" => _colourSetForTheme.KeywordBlue,
            "keyword - control" => _colourSetForTheme.KeywordBlue,
            "preprocessor keyword" => _colourSetForTheme.KeywordBlue,

            // Literals & comments
            "string" => _colourSetForTheme.LightOrangeBrown,
            "string - verbatim" => _colourSetForTheme.LightOrangeBrown,
            "string - escape character" => _colourSetForTheme.Orange,
            "comment" => _colourSetForTheme.CommentGreen,
            "number" => _colourSetForTheme.NumberGreen,

            // Types (User Types)
            "class name" => _colourSetForTheme.ClassGreen,
            "record class name" => _colourSetForTheme.ClassGreen,
            "struct name" => _colourSetForTheme.ClassGreen,
            "record struct name" => _colourSetForTheme.ClassGreen,
            "interface name" => _colourSetForTheme.InterfaceGreen,
            "enum name" => _colourSetForTheme.InterfaceGreen,
            "namespace name" => _colourSetForTheme.White,
            
            // Identifiers & members
            "identifier" => _colourSetForTheme.White,
            "constant name" => _colourSetForTheme.White,
            "enum member name" => _colourSetForTheme.White,
            "method name" => _colourSetForTheme.Yellow,
            "extension method name" => _colourSetForTheme.Yellow,
            "property name" => _colourSetForTheme.White,
            "field name" => _colourSetForTheme.White,
            "static symbol" => _colourSetForTheme.Yellow, // ??
            "parameter name" => _colourSetForTheme.VariableBlue,
            "local name" => _colourSetForTheme.VariableBlue,
            "type parameter name" => _colourSetForTheme.ClassGreen,
            "delegate name" => _colourSetForTheme.ClassGreen,
            "event name" => _colourSetForTheme.White,
            "label name" => _colourSetForTheme.White,

            // Punctuation & operators
            "operator" => _colourSetForTheme.White,
            "operator - overloaded" => _colourSetForTheme.Yellow,
            "punctuation" => _colourSetForTheme.White,
            
            // Preprocessor
            "preprocessor text" => _colourSetForTheme.White,
            
            // Xml comments
            "xml doc comment - delimiter" => _colourSetForTheme.CommentGreen,
            "xml doc comment - name" => _colourSetForTheme.White,
            "xml doc comment - text" => _colourSetForTheme.CommentGreen,
            "xml doc comment - attribute name" => _colourSetForTheme.LightOrangeBrown,
            "xml doc comment - attribute quotes" => _colourSetForTheme.LightOrangeBrown,

            // Misc
            "excluded code" => _colourSetForTheme.Gray,

            _ => _colourSetForTheme.Pink // pink, warning color for unhandled classifications
        };
        if (colour == _colourSetForTheme.Pink)
        {
            GD.PrintErr($"Unhandled classification type: '{classificationType}'");
        }
        return colour;
    }
}

public static class CachedColors
{
    public static readonly Color Orange = new("f27718");
    public static readonly Color White = new("dcdcdc");
    public static readonly Color Yellow = new("dcdcaa");
    public static readonly Color CommentGreen = new("57a64a");
    public static readonly Color KeywordBlue = new("569cd6");
    public static readonly Color LightOrangeBrown = new("d69d85");
    public static readonly Color NumberGreen = new("b5cea8");
    public static readonly Color InterfaceGreen = new("b8d7a3");
    public static readonly Color ClassGreen = new("4ec9b0");
    public static readonly Color VariableBlue = new("9cdcfe");
    public static readonly Color Gray = new("a9a9a9");
    public static readonly Color Pink = new("c586c0");
    public static readonly Color ErrorRed = new("da5b5a");
    
    public static readonly Color RazorComponentGreen = new("0b7f7f");
    public static readonly Color RazorMetaCodePurple = new("a699e6");
    public static readonly Color HtmlDelimiterGray = new("808080");
}

public static class CachedColorsLight
{
    public static readonly Color Orange = new("b776fb"); //
    public static readonly Color White = new("000000"); //
    public static readonly Color Yellow = new("74531f"); //
    public static readonly Color CommentGreen = new("008000"); //
    public static readonly Color KeywordBlue = new("0000ff"); //
    public static readonly Color LightOrangeBrown = new("a31515"); //
    public static readonly Color NumberGreen = new("000000"); //
    public static readonly Color InterfaceGreen = new("2b91af"); //
    public static readonly Color ClassGreen = new("2b91af"); //
    public static readonly Color VariableBlue = new("1f377f"); //
    public static readonly Color Gray = new("a9a9a9"); //
    public static readonly Color Pink = new("c586c0"); //
    public static readonly Color ErrorRed = new("da5b5a"); //
    
    public static readonly Color RazorComponentGreen = new("0b7f7f");
    public static readonly Color RazorMetaCodePurple = new("826ee6");
    public static readonly Color HtmlDelimiterGray = new("808080");
}