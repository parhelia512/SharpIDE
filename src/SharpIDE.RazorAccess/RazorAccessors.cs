extern alias WorkspaceAlias;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Razor.Language;
using RazorCodeDocumentExtensions = WorkspaceAlias::Microsoft.AspNetCore.Razor.Language.RazorCodeDocumentExtensions;

namespace SharpIDE.RazorAccess;

public static class RazorAccessors
{
	//private static RazorProjectEngine? _razorProjectEngine;

	// I didn't end up using the Razor ClassifiedSpans
	public static (ImmutableArray<SharpIdeRazorClassifiedSpan>, List<SharpIdeRazorSourceMapping>) GetSpansAndMappingsForRazorCodeDocument(RazorCodeDocument razorCodeDocument, RazorCSharpDocument razorCSharpDocument)
	{
		var razorSpans = RazorCodeDocumentExtensions.GetClassifiedSpans(razorCodeDocument);
		var sharpIdeSpans = razorSpans.Select(s => new SharpIdeRazorClassifiedSpan(s.Span.ToSharpIdeSourceSpan(), s.Kind.ToSharpIdeSpanKind())).ToList();

		var result = (sharpIdeSpans.ToImmutableArray(), razorCSharpDocument.SourceMappings.Select(s => s.ToSharpIdeSourceMapping()).ToList());
		return result;
	}

	// public static ImmutableArray<RazorCodeDocumentExtensions.ClassifiedSpan> GetClassifiedSpansForRazorCodeDocument(RazorCodeDocument razorCodeDocument)
	// {
	// 	var razorSpans = RazorCodeDocumentExtensions.GetClassifiedSpans(razorCodeDocument);
	// 	return razorSpans;
	// }

	// public static (ImmutableArray<SharpIdeRazorClassifiedSpan>, SourceText Text, List<SharpIdeRazorSourceMapping>) GetClassifiedSpans(SourceText sourceText, SourceText importsSourceText, string razorDocumentFilePath, string projectDirectory)
	// {
	// 	var razorSourceDocument = RazorSourceDocument.Create(sourceText.ToString(), razorDocumentFilePath);
	// 	var importsRazorSourceDocument = RazorSourceDocument.Create(importsSourceText.ToString(), "_Imports.razor");
	//
	// 	var razorProjectFileSystem = RazorProjectFileSystem.Create(projectDirectory);
	// 	_razorProjectEngine ??= RazorProjectEngine.Create(RazorConfiguration.Default, razorProjectFileSystem,
	// 		builder => { /* configure features if needed */ });
	// 	//var projectItem = razorProjectFileSystem.GetItem(razorDocumentFilePath, RazorFileKind.Component);
	//
	// 	//var razorCodeDocument = projectEngine.Process(razorSourceDocument, RazorFileKind.Component, [], []);
	// 	var razorCodeDocument = _razorProjectEngine.Process(razorSourceDocument, RazorFileKind.Component, [importsRazorSourceDocument], []);
	// 	var razorCSharpDocument = razorCodeDocument.GetRequiredCSharpDocument();
	// 	//var generatedSourceText = razorCSharpDocument.Text;
	//
	// 	//var filePath = razorCodeDocument.Source.FilePath.AssumeNotNull();
	// 	//var razorSourceText = razorCodeDocument.Source.Text;
	// 	var razorSpans = RazorCodeDocumentExtensions.GetClassifiedSpans(razorCodeDocument);
	//
	// 	//var sharpIdeSpans = MemoryMarshal.Cast<RazorCodeDocumentExtensions.ClassifiedSpan, SharpIdeRazorClassifiedSpan>(razorSpans);
	// 	var sharpIdeSpans = razorSpans.Select(s => new SharpIdeRazorClassifiedSpan(s.Span.ToSharpIdeSourceSpan(), s.Kind.ToSharpIdeSpanKind())).ToList();
	//
	// 	var result = (sharpIdeSpans.ToImmutableArray(), razorCSharpDocument.Text, razorCSharpDocument.SourceMappings.Select(s => s.ToSharpIdeSourceMapping()).ToList());
	// 	return result;
	// }

	// public static bool TryGetMappedSpans(
	// 	TextSpan span,
	// 	SourceText source,
	// 	RazorCSharpDocument output,
	// 	out LinePositionSpan linePositionSpan,
	// 	out TextSpan mappedSpan)
	// {
	// 	foreach (SourceMapping sourceMapping in output.SourceMappings)
	// 	{
	// 		TextSpan textSpan1 = sourceMapping.OriginalSpan.AsTextSpan();
	// 		TextSpan textSpan2 = sourceMapping.GeneratedSpan.AsTextSpan();
	// 		if (textSpan2.Contains(span))
	// 		{
	// 			int num1 = span.Start - textSpan2.Start;
	// 			int num2 = span.End - textSpan2.End;
	// 			if (num1 >= 0 && num2 <= 0)
	// 			{
	// 				mappedSpan = new TextSpan(textSpan1.Start + num1, textSpan1.End + num2 - (textSpan1.Start + num1));
	// 				linePositionSpan = source.Lines.GetLinePositionSpan(mappedSpan);
	// 				return true;
	// 			}
	// 		}
	// 	}
	// 	mappedSpan = new TextSpan();
	// 	linePositionSpan = new LinePositionSpan();
	// 	return false;
	// }
}
