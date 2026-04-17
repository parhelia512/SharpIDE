using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using Ardalis.GuardClauses;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Extensions.Logging;
using NeoSmart.AsyncLock;
using PolyType.SourceGenerator;
using SharpIDE.Application.Features.Events;
using SharpIDE.Application.Features.Logging;
using SharpIDE.MsBuildHost.Contracts;
using StreamJsonRpc;
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

public class BuildService(ILogger<BuildService> logger) : IDisposable
{
	private readonly ILogger<BuildService> _logger = logger;

	public EventWrapper<BuildStartedFlags, Task> BuildStarted { get; } = new(_ => Task.CompletedTask);
	public EventWrapper<Task> BuildFinished { get; } = new(() => Task.CompletedTask);
	public PipeReader? BuildLogPipeReader { get; private set; }
	private Task? _fillPipeFromLoggerTask;
	private Process? _sharpIdeMsBuildHostProcess;
	private Socket? _buildLogSocket;
	private CancellationTokenSource? _cancellationTokenSource;
	private IRpcBuildService? _rpcBuildService;
	private readonly AsyncLock _rpcInitLock = new AsyncLock();

	private async Task<IRpcBuildService> ConnectRpc()
	{
		using (await _rpcInitLock.LockAsync())
		{
			if (_rpcBuildService is not null) return _rpcBuildService;
			if (_fillPipeFromLoggerTask is not null) throw new InvalidOperationException("Build logger pipe is already open, but RPC service is not initialized. This should never happen.");
			var sharpIdeMsBuildHostDllPath = Path.Combine(AppContext.BaseDirectory, "SharpIdeMsBuildHost", "SharpIDE.MsBuildHost.dll");
			var startupInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = sharpIdeMsBuildHostDllPath,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			startupInfo.Environment["DOTNET_ROLL_FORWARD_TO_PRERELEASE"] = "1";
			var process = Process.Start(startupInfo);
			if (process is null) throw new InvalidOperationException("Failed to start SharpIDE.MsBuildHost");
			var handler = new LengthHeaderMessageHandler(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, new NerdbankMessagePackFormatter { TypeShapeProvider = TypeShapeProvider_SharpIDE_MsBuildHost_Contracts.Default });
			var rpc = new JsonRpc(handler);

			rpc.StartListening();

			var proxy = rpc.Attach<IRpcBuildService>();
			_fillPipeFromLoggerTask = await OpenMsBuildLoggerPipe(proxy);
			_sharpIdeMsBuildHostProcess = process;
			return proxy;
		}
	}
	public async Task<SharpIdeBuildResult> MsBuildAsync(string solutionOrProjectFilePath, BuildType buildType = BuildType.Build, BuildStartedFlags buildStartedFlags = BuildStartedFlags.UserFacing, CancellationToken cancellationToken = default)
	{
		_rpcBuildService ??= await ConnectRpc();
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

	private async Task<Task> OpenMsBuildLoggerPipe(IRpcBuildService rpcBuildService)
	{
		if (_buildLogSocket is not null) throw new InvalidOperationException("Build log socket is already open.");
		var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
		var pipeReader = pipe.Reader;
		var pipeWriter = pipe.Writer;

		var unixDomainSocketFilePath = $"{Path.GetTempFileName()}.sock";
		var serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		serverSocket.Bind(new UnixDomainSocketEndPoint(unixDomainSocketFilePath));
		serverSocket.Listen(1);
		await rpcBuildService.BeginWritingMsBuildOutputToSocket(unixDomainSocketFilePath);

		var socket = await serverSocket.AcceptAsync();
		serverSocket.Close();
		File.Delete(unixDomainSocketFilePath);
		var fillPipeTask = Task.Run(async () =>
		{
			try
			{
				var buffer = new byte[4096];
				while (true)
				{
					var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);
					if (bytesRead == 0) break; // End of stream
					await pipeWriter.WriteAsync(buffer.AsMemory(0, bytesRead));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while reading from SharpIDE.MsBuildHost logger socket");
			}
			finally
			{
				await pipeWriter.CompleteAsync();
				socket.Dispose();
			}
		});
		BuildLogPipeReader = pipeReader;
		_buildLogSocket = socket;
		return fillPipeTask;
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
