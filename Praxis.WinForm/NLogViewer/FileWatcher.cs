namespace Praxis.WinForm.NLogViewer;

using System;
using System.Threading.Tasks;

/// <summary>
/// Class used that periodically watches for files and changes in those files
/// </summary>
/// <remarks>
/// Creates an instance of the <see cref="FileWatcher"/> class.
/// </remarks>
/// <param name="directory">The directory to watch. Subdirectories underneath this directory are also watched.</param>
/// <param name="tickFrequency">How often to process file target changes.</param>
/// <param name="startupDelay">Delay in initial startup to process changes / existing files.</param>
/// <param name="pollingLoopOnException">Delegate for errors during the polling loop.</param>
/// <param name="processLine">Used to signal that there is a line of text to be processed.</param>
/// <param name="startingRead">Delegate called before reading lines from files.</param>
/// <param name="finishedRead">Delegate called after reading lines from files has finished.</param>
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
internal class FileWatcher(string directory, TimeSpan tickFrequency, TimeSpan startupDelay, Action<Exception> pollingLoopOnException, Action<string> processLine, Action startingRead, Action finishedRead) : PeriodicTimerContainer(tickFrequency, startupDelay, pollingLoopOnException) {
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

	/// <summary>
	/// Holds file fullnames and their current "up to" read position.
	/// </summary>
	private readonly Dictionary<string, long> _filePositions = [];

	protected override Task Tick(CancellationToken cToken) {
		bool startedRead = false;

		foreach (var fileInfo in _RecursiveInDirectory(directory)) {
			string fullName = fileInfo.FullName;
			long length = fileInfo.Length;
			long curPosition = _filePositions.GetOrAdd(fullName, 0);

			if (curPosition == length)
				continue;

			if (!startedRead) {
				startedRead = true;
				startingRead();
			}

			try {
				// Read and update
				using var fileStream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				fileStream.Position = curPosition;

				using var reader = new StreamReader(fileStream);
				string? line;
				while ((line = reader.ReadLine()) != null) {
					processLine(line);
				}

				_filePositions[fullName] = fileStream.Position;
			}
			catch (Exception ex) {
				pollingLoopOnException(new Exception($"Could not read from: [{fullName}]. Tree results may be inaccurate but operation will continue.", ex.AddData(fullName)));
			}
		}

		if (startedRead)
			finishedRead();

		return Task.CompletedTask;
	}

	/* ----------------------------------------------------------------------------------------------------------
 * Gets all files recursively underneath a target directory */
	private static FileInfo[] _RecursiveInDirectory(string directory) => new DirectoryInfo(directory).GetFiles("*.json", SearchOption.AllDirectories);
}
