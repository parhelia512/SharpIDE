using System.Diagnostics;

namespace SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

public static class VsPersistenceMapper
{
	public static async Task<SharpIdeSolutionModel> GetSolutionModel(string solutionFilePath, CancellationToken cancellationToken = default)
	{
		using var _ = SharpIdeOtel.Source.StartActivity();
		var timer = Stopwatch.StartNew();
		// This intermediate model is pretty much useless, but I have left it around as we grab the project nodes with it, which we might use later.
		var intermediateModel = await IntermediateMapper.GetIntermediateModel(solutionFilePath, cancellationToken);

		var solutionModel = new SharpIdeSolutionModel(solutionFilePath, intermediateModel);

		timer.Stop();
		Console.WriteLine($"Solution model fully created in {timer.ElapsedMilliseconds} ms");

		return solutionModel;
	}
}
