using System.Collections.Concurrent;
using System.Collections.Immutable;
using FileWatcherEx;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.CodeAnalysis.Threading;
using SharpIDE.Application.Features.Events;

namespace SharpIDE.Application.Features.FileWatching;

public class AnalyzerFileWatcher
{
	private readonly AsyncBatchingWorkQueue<string> _fileChangedQueue;

	public AnalyzerFileWatcher()
	{
		_fileChangedQueue = new AsyncBatchingWorkQueue<string>(
			TimeSpan.FromMilliseconds(500),
			async (filePaths, ct) => await GlobalEvents.Instance.AnalyzerDllsChanged.InvokeParallelAsync(filePaths.ToImmutableArray()),
			new AsynchronousOperationListenerProvider.NullOperationListener(),
			CancellationToken.None);
	}

	private readonly ConcurrentDictionary<string, FileSystemWatcherEx> _fileWatchers = new();

	// I wanted to avoid this, but unfortunately we have to watch individual files in different directories
	public async Task StartWatchingFiles(ImmutableArray<string> filePaths)
	{
		// This can definitely be optimized to not recreate watchers unnecessarily
		var existingFileWatchers = _fileWatchers.Values.ToList();
		foreach (var watcher in existingFileWatchers)
		{
			watcher.OnChanged -= OnFileChanged;
			watcher.Dispose();
		}
		_fileWatchers.Clear();

		foreach (var filePath in filePaths)
		{
			var fileWatcher = new FileSystemWatcherEx(Path.GetDirectoryName(filePath)!)
			{
				Filter = Path.GetFileName(filePath),
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
			};

			fileWatcher.OnChanged += OnFileChanged;
			fileWatcher.Start();
			_fileWatchers.TryAdd(filePath, fileWatcher);
		}
	}

	private void OnFileChanged(object? sender, FileChangedEvent e)
	{
		_fileChangedQueue.AddWork(e.FullPath);
	}
}
