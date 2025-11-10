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
/// <param name="processLines">Used to signal that there are lines of text to be processed.</param>
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
internal class FileWatcher(string directory, TimeSpan tickFrequency, TimeSpan startupDelay, Action<Exception> pollingLoopOnException, Action<Queue<string>> processLines) : PeriodicTimerContainer(tickFrequency, startupDelay, pollingLoopOnException) {
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

	/// <summary>
	/// Holds file fullnames and their current "up to" read position.
	/// </summary>
	private readonly Dictionary<string, long> _filePositions = [];

	protected override async Task Tick(CancellationToken cToken) {
		Queue<string> lines = [];

		foreach (var fileInfo in _RecursiveInDirectory(directory)) {
			string fullName = fileInfo.FullName;
			long length = fileInfo.Length;
			long curPosition = _filePositions.GetOrAdd(fullName, 0);

			if (curPosition == length)
				continue;

			try {
				// Read and update
				using var fileStream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				fileStream.Position = curPosition;

				using var reader = new StreamReader(fileStream);
				string? line;
				while ((line = (await reader.ReadLineAsync(cToken).ConfigureAwait(false))) is not null)
					lines.Enqueue(line);

				_filePositions[fullName] = fileStream.Position;
			}
			catch (Exception ex) {
				pollingLoopOnException(new Exception($"Could not read from: [{fullName}]. Tree results may be inaccurate but operation will continue.", ex.AddData(fullName)));
			}
		}

		if(lines.Count > 0)
			processLines(lines);
	}

	/* ----------------------------------------------------------------------------------------------------------
 * Gets all files recursively underneath a target directory */
	private static FileInfo[] _RecursiveInDirectory(string directory) => new DirectoryInfo(directory).GetFiles("*.json", SearchOption.AllDirectories);
}
