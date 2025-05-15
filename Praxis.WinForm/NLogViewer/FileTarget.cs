namespace Praxis.WinForm.NLogViewer;

/// <summary>
/// Represents a target file that is being watched and tracks position and whether it had changes
/// </summary>
internal class FileTarget(string name) {

	/// <summary>
	/// Gets or sets whether or not this file has had a change in it's contents
	/// </summary>
	public bool HadChange { get; set; } = true;

	/// <summary>
	/// Gets or sets the last position the file was read to
	/// </summary>
	public long LastPosition { get; set; }

	/// <summary>
	/// Gets or inits the actual file name
	/// </summary>
	public string Name { get; init; } = name;
}