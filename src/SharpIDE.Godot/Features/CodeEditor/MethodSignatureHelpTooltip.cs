using System.Collections.Immutable;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.QuickInfo.Presentation;
using Microsoft.CodeAnalysis.SignatureHelp;

namespace SharpIDE.Godot.Features.CodeEditor;

public static class MethodSignatureHelpTooltip
{
	private static readonly FontVariation MonospaceFont = ResourceLoader.Load<FontVariation>("uid://cctwlwcoycek7");
	private static readonly Color HrColour = new Color("4d4d4d");
	
	public static void WriteToMethodSignatureHelpLabel(RichTextLabel richTextLabel, SignatureHelpItems signatureHelpItems, EditorThemeColorSet editorThemeColorSet)
	{
		for (var i = 0; i < signatureHelpItems.Items.Count; i++)
		{
			var item = signatureHelpItems.Items[i];
			var prefixQuickInfoElements = item.PrefixDisplayParts.ToInteractiveTextElements(null);
			foreach (var quickInfoElement in prefixQuickInfoElements)
			{
				CompletionDescriptionTooltip.WriteQuickInfoElement(richTextLabel, quickInfoElement, editorThemeColorSet);
			}
			var parameters = item.Parameters;
			var paramSeparatorString = item.SeparatorDisplayParts.GetFullText();
			for (var j = 0; j < parameters.Length; j++)
			{
				var parameter = parameters[j];
				var parameterQuickInfoElements = parameter.DisplayParts.ToImmutableArray().ToInteractiveTextElements(null);
				foreach (var quickInfoElement in parameterQuickInfoElements)
				{
					var isCurrentParameter = j == signatureHelpItems.SemanticParameterIndex;
					CompletionDescriptionTooltip.WriteQuickInfoElement(richTextLabel, quickInfoElement, editorThemeColorSet, isCurrentParameter);
				}
				if (j < parameters.Length - 1)
				{
					richTextLabel.PushFont(MonospaceFont);
					richTextLabel.AppendText(paramSeparatorString);
					richTextLabel.Pop();
				}
			}
            
			var suffixQuickInfoElements = item.SuffixDisplayParts.ToInteractiveTextElements(null);
			foreach (var quickInfoElement in suffixQuickInfoElements)
			{
				CompletionDescriptionTooltip.WriteQuickInfoElement(richTextLabel, quickInfoElement, editorThemeColorSet);
			}
			richTextLabel.Newline();
			var documentationQuickInfoElements = item.DocumentationFactory(CancellationToken.None).ToImmutableArray().ToInteractiveTextElements(null);
			foreach (var quickInfoElement in documentationQuickInfoElements)
			{
				CompletionDescriptionTooltip.WriteQuickInfoElement(richTextLabel, quickInfoElement, editorThemeColorSet);
			}
			if (i < signatureHelpItems.Items.Count - 1)
			{
				richTextLabel.Newline();
				richTextLabel.AddHr(100, 1, HrColour, HorizontalAlignment.Center, true);
				richTextLabel.Newline();
			}
		}
	}
}