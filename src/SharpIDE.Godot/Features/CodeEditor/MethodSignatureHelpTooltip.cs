using System.Collections.Immutable;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.QuickInfo.Presentation;
using Microsoft.CodeAnalysis.SignatureHelp;

namespace SharpIDE.Godot.Features.CodeEditor;

public static class MethodSignatureHelpTooltip
{
	private static readonly FontVariation MonospaceFont = ResourceLoader.Load<FontVariation>("uid://cctwlwcoycek7");
	
	public static void WriteToMethodSignatureHelpLabel(RichTextLabel richTextLabel, SignatureHelpItems signatureHelpItems, EditorThemeColorSet editorThemeColorSet)
	{
		for (var i = 0; i < signatureHelpItems.Items.Count; i++)
		{
			var item = signatureHelpItems.Items[i];
			var isCurrentItem = i == signatureHelpItems.SelectedItemIndex;
			//if (isCurrentItem) richTextLabel.PushBold();
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
					CompletionDescriptionTooltip.WriteQuickInfoElement(richTextLabel, quickInfoElement, editorThemeColorSet);
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
			//if (isCurrentItem) richTextLabel.Pop();
			richTextLabel.AppendText("\n");
		}
	}
}