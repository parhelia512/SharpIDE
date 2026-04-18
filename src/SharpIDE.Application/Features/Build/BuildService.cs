using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SharpIDE.Application.Features.Events;
using SharpIDE.MsBuildHost.Contracts;

// ReSharper disable InconsistentlySynchronizedField

namespace SharpIDE.Application.Features.Build;

public enum BuildType
{
	Build,
	Rebuild,
	Clean,
	Restore
}
public enum BuildStartedFlags { UserFacing = 0, Internal }
public enum SharpIdeBuildResult { Success = 0, Failure }

public partial class BuildService(ILogger<BuildService> logger) : IDisposable
{
	private readonly ILogger<BuildService> _logger = logger;

	public EventWrapper<BuildStartedFlags, Task> BuildStarted { get; } = new(_ => Task.CompletedTask);
	public EventWrapper<Task> BuildFinished { get; } = new(() => Task.CompletedTask);
	public PipeReader? BuildLogPipeReader { get; private set; }

	private CancellationTokenSource? _cancellationTokenSource;
	private IRpcBuildService? _rpcBuildService;

	public async Task<SharpIdeBuildResult> MsBuildAsync(string solutionOrProjectFilePath, BuildType buildType = BuildType.Build, BuildStartedFlags buildStartedFlags = BuildStartedFlags.UserFacing, CancellationToken cancellationToken = default)
	{
		_rpcBuildService ??= await ConnectRpc(solutionOrProjectFilePath);
		if (_cancellationTokenSource is not null) throw new InvalidOperationException("A build is already in progress.");
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		using var _ = SharpIdeOtel.Source.StartActivity($"{nameof(BuildService)}.{nameof(MsBuildAsync)}");

		BuildStarted.InvokeParallelFireAndForget(buildStartedFlags);
		var timer = Stopwatch.StartNew();
		var buildTypeDto = buildType switch
		{
			BuildType.Build => BuildTypeDto.Build,
			BuildType.Rebuild => BuildTypeDto.Rebuild,
			BuildType.Clean => BuildTypeDto.Clean,
			BuildType.Restore => BuildTypeDto.Restore,
			_ => throw new ArgumentOutOfRangeException(nameof(buildType), buildType, null)
		};
		var (buildResult, exception) = await _rpcBuildService.MsBuildAsync(solutionOrProjectFilePath, buildTypeDto, _cancellationTokenSource.Token).ConfigureAwait(false);
		timer.Stop();
		BuildFinished.InvokeParallelFireAndForget();
		_cancellationTokenSource = null;
		_logger.LogInformation(exception, "Build result: {BuildResult} in {ElapsedMilliseconds}ms", buildResult, timer.ElapsedMilliseconds);
		var mappedResult = buildResult switch
		{
			BuildResultDto.Success => SharpIdeBuildResult.Success,
			BuildResultDto.Failure => SharpIdeBuildResult.Failure,
			_ => throw new ArgumentOutOfRangeException()
		};
		return mappedResult;
	}

	public async Task CancelBuildAsync()
	{
		if (_cancellationTokenSource is null) throw new InvalidOperationException("No build is in progress.");
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource = null;
	}

	public void Dispose()
	{
		_cancellationTokenSource?.Cancel();
		_buildLogSocket?.Shutdown(SocketShutdown.Both);
		_buildLogSocket?.Close(0);
		_sharpIdeMsBuildHostProcess?.Kill(entireProcessTree: true);
		_sharpIdeMsBuildHostProcess?.Dispose();
	}
}
