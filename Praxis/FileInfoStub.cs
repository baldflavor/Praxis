namespace Praxis;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Used to provide a non-recursive / subset of information gleaned from a <see cref="FileInfo"/> instance
/// </summary>
/// <param name="CreationTime">When the file was created</param>
/// <param name="CreationTimeUtc">When the file was created in UTC</param>
/// <param name="Exists">Whether the file exists</param>
/// <param name="FullName">The full file path</param>
/// <param name="IsReadOnly">Whether the file is read only</param>
/// <param name="LastWriteTime">The time the file was last written to</param>
/// <param name="LastWriteTimeUtc">The time the file was last written to in UTC</param>
/// <param name="Length">The size of the file in bytes; null if the file does not exist or length cannot be determined</param>
public record class FileInfoStub(DateTime CreationTime, DateTime CreationTimeUtc, bool Exists, string FullName, bool IsReadOnly, DateTime LastWriteTime, DateTime LastWriteTimeUtc, long? Length) {

	/// <summary>
	/// Creates an instance of this record using the supplied argument
	/// </summary>
	/// <param name="fi"><see cref="FileInfo"/> to use for source data</param>
	/// <returns><see cref="FileInfoStub"/></returns>
	[return: NotNullIfNotNull(nameof(fi))]
	public static FileInfoStub? From(FileInfo? fi) {
		if (fi == null)
			return default;

		long? length;
		try {
			length = fi.Exists ? fi.Length : null;
		}
		catch (Exception) {
			length = null;
		}

		return new FileInfoStub(
				fi.CreationTime,
				fi.CreationTimeUtc,
				fi.Exists,
				fi.FullName,
				fi.IsReadOnly,
				fi.LastWriteTime,
				fi.LastWriteTimeUtc,
				length);
	}

	/// <summary>
	/// Creates a <see cref="FileInfo"/> instance using <see cref="FullName"/> as a constructor value
	/// </summary>
	/// <returns><see cref="FileInfo"/></returns>
	/// <exception cref="Exception">May be thrown given security, path or argument exceptions thrown by the <see cref="FileInfo"/> constructor</exception>
	public FileInfo ToFileInfo() => new(this.FullName);

	/// <summary>
	/// Represents an empty instance -- boolean fields are all false, dates and times are all minimum value, fullname is an empty string and length is null
	/// </summary>
	public static FileInfoStub Empty { get; } = new(DateTime.MinValue, DateTime.MinValue, false, "", false, DateTime.MinValue, DateTime.MinValue, null);
}
