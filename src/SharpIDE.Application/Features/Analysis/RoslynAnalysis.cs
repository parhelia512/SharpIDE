using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.MSBuild;

namespace SharpIDE.Application.Features.Analysis;

public static class RoslynAnalysis
{
	private static MSBuildWorkspace? _workspace;
	public static void StartSolutionAnalysis(string solutionFilePath)
	{
		_ = Task.Run(async () =>
		{
			try
			{
				await Analyse(solutionFilePath);
			}
			catch (Exception e)
			{
				Console.WriteLine($"RoslynAnalysis: Error during analysis: {e}");
			}
		});
	}
	public static async Task Analyse(string solutionFilePath)
	{
		Console.WriteLine($"RoslynAnalysis: Loading solution");
		var timer = Stopwatch.StartNew();
		_workspace ??= MSBuildWorkspace.Create();
		_workspace.WorkspaceFailed += (o, e) => throw new InvalidOperationException($"Workspace failed: {e.Diagnostic.Message}");
		var solution = await _workspace.OpenSolutionAsync(solutionFilePath, new Progress());
		timer.Stop();
		Console.WriteLine($"RoslynAnalysis: Solution loaded in {timer.ElapsedMilliseconds}ms");
		Console.WriteLine();

		foreach (var project in solution.Projects)
		{
			//Console.WriteLine($"Project: {project.Name}");
			var compilation = await project.GetCompilationAsync();
			Guard.Against.Null(compilation, nameof(compilation));

			// Get diagnostics (built-in or custom analyzers)
			var diagnostics = compilation.GetDiagnostics();
			var nonHiddenDiagnostics = diagnostics.Where(d => d.Severity is not Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden).ToList();

			foreach (var diagnostic in nonHiddenDiagnostics)
			{
				Console.WriteLine(diagnostic);
				// Optionally run CodeFixProviders here
			}
			foreach (var document in project.Documents)
			{
				// var syntaxTree = await document.GetSyntaxTreeAsync();
				// var root = await syntaxTree!.GetRootAsync();
				// var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, root.FullSpan);
				// foreach (var span in classifiedSpans)
				// {
				// 	var classifiedSpan = root.GetText().GetSubText(span.TextSpan);
				// 	Console.WriteLine($"{span.TextSpan}: {span.ClassificationType}");
				// 	Console.WriteLine(classifiedSpan);
				// }
			}
		}
	}
}
