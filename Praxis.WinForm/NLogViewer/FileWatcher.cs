namespace Praxis.WinForm.NLogViewer;

using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Class used that periodically watches for files and changes in those files
/// </summary>
internal class FileWatcher : PeriodicTimerContainer {

	/// <summary>
	/// Holds file paths, their current up to date position, and whether or not changes need to be processed
	/// </summary>
	private readonly List<FileTarget> _fileTargets;

	/// <summary>
	/// Delegate that runs when content from all files has been read
	/// </summary>
	private readonly Action _finishedRead;

	/// <summary>
	/// File system watcher for updating files whether created or changed
	/// </summary>
	private readonly FileSystemWatcher _fsw;

	/// <summary>
	/// Delegate that is executed for each line read from a file
	/// </summary>
	private readonly Action<string> _processLine;

	/// <summary>
	/// Delegate that runs when reading content from new files has started
	/// </summary>
	private readonly Action _startingRead;


	/// <summary>
	/// Creates an instance of the <see cref="FileWatcher"/> class.
	/// </summary>
	/// <param name="directory">The directory to watch. Subdirectories underneath this directory are also watched.</param>
	/// <param name="tickFrequency">How often to process file changes.</param>
	/// <param name="startupDelay">Delay in initial startup to process changes / existing files.</param>
	/// <param name="pollingLoopOnException">Delegate for errors during the polling loop.</param>
	/// <param name="processLine">Used to signal that there is a line of text to be processed.</param>
	/// <param name="startingRead">Delegate called before reading lines from files.</param>
	/// <param name="finishedRead">Delegate called after reading lines from files has finished.</param>
	public FileWatcher(string directory, TimeSpan tickFrequency, TimeSpan startupDelay, Action<Exception> pollingLoopOnException, Action<string> processLine, Action startingRead, Action finishedRead) : base(tickFrequency, startupDelay, pollingLoopOnException) {
		_fileTargets = [.. Directory.GetFiles(directory, "*", new EnumerationOptions { RecurseSubdirectories = true }).Select(f => new FileTarget(f))];

		_fsw = new FileSystemWatcher(directory) { IncludeSubdirectories = true };
		_fsw.Created += (s, e) => {
			lock (_fileTargets)
				_fileTargets.Add(new FileTarget(e.FullPath));
		};

		_fsw.Changed += (s, e) => {
			lock (_fileTargets)
				_fileTargets.Single(f => f.Name == e.FullPath).HadChange = true;
		};

		_fsw.EnableRaisingEvents = true;

		_processLine = processLine;
		_startingRead = startingRead;
		_finishedRead = finishedRead;
	}



	protected override Task Tick(CancellationToken cToken) {
		FileTarget[] toProcess;
		lock (_fileTargets) {
			toProcess = [.. _fileTargets.Where(f => f.HadChange)];
			if (toProcess.Length == 0)
				return Task.CompletedTask;
		}

		_startingRead();

		foreach (var file in toProcess) {
			file.HadChange = false;

			// Read and update
			using FileStream fileStream = new FileStream(file.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			fileStream.Position = file.LastPosition;

			using StreamReader reader = new StreamReader(fileStream);
			string? line;
			while ((line = reader.ReadLine()) != null) {
				_processLine(line);
			}

			file.LastPosition = fileStream.Position;
		}

		_finishedRead();

		return Task.CompletedTask;
	}
}
