namespace Praxis.IO;

using System.IO.Compression;

/// <summary>
/// Static methods for utilizing built in compression algorithms.
/// </summary>
/// <remarks>
/// Compression / decompression all utilizes <c>Brotli</c>. This was chosen because when using the <see cref="CompressionLevel.Optimal"/>
/// setting, repeatedly performs the fastest with compression levels nearing that of <see cref="CompressionLevel.SmallestSize"/>. As such,
/// these methods all use <see cref="CompressionLevel.Optimal"/> internally.
/// </remarks>
public static class Compression {

	/// <summary>
	/// Applies compression to a byte array.
	/// </summary>
	/// <param name="arg">Bytes to be compressed.</param>
	/// <returns>Compressed representation of source bytes.</returns>
	public static byte[] Compress(byte[] arg) {
		using MemoryStream ms = new();
		using BrotliStream cs = new(ms, CompressionLevel.Optimal);
		cs.Write(arg, 0, arg.Length);
		cs.Flush();
		return ms.ToArray();
	}

	/// <summary>
	/// Compresses the contents of a stream.
	/// </summary>
	/// <param name="inputStream">Stream used as input for compression. Does not set position before reading.</param>
	/// <param name="outStream">Stream where compressed output is written. Does not set position before writing.</param>
	/// <param name="bufferSizeKB">Number of kilobytes to use for the buffer during read/write.</param>
	/// <returns>Task</returns>
	/// <exception cref="Exception">May be thrown if the input stream cannot be read or a failure occurs during compression.</exception>
	public static async Task Compress(Stream inputStream, Stream outStream, int bufferSizeKB = 64) {
		ArgumentOutOfRangeException.ThrowIfLessThan(bufferSizeKB, 1);

		using var compStream = new BrotliStream(outStream, CompressionLevel.Optimal);
		byte[] buffer = new byte[bufferSizeKB * Const.KILOBYTE];

		int bytesRead;
		while ((bytesRead = await inputStream.ReadAsync(buffer).ConfigureAwait(false)) > 0) {
			await compStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
		}
	}


	/// <summary>
	/// Decompresses the source data into an array of bytes
	/// </summary>
	/// <param name="source">Source data to decompress</param>
	/// <returns>A decompressed byte array</returns>
	public static byte[] Decompress(byte[] source) {
		using MemoryStream sourceMs = new(source);
		sourceMs.Position = 0;

		using BrotliStream compStream = new(sourceMs, CompressionMode.Decompress);

		using MemoryStream returnMs = new();
		compStream.CopyTo(returnMs);

		return returnMs.ToArray();
	}

	/// <summary>
	/// Compresses the contents of a stream.
	/// </summary>
	/// <param name="inputStream">Stream used as input for decompression. Does not set position before reading.</param>
	/// <param name="outStream">Stream where decompressed output is written. Does not set position before writing.</param>
	/// <param name="bufferSizeKB">Number of kilobytes to use for the buffer during read/write.</param>
	/// <returns>Task</returns>
	/// <exception cref="Exception">May be thrown if the input stream cannot be read or a failure occurs during decompression.</exception>
	public static async Task Decompress(Stream inputStream, Stream outStream, int bufferSizeKB = 64) {
		ArgumentOutOfRangeException.ThrowIfLessThan(bufferSizeKB, 1);

		using var compStream = new BrotliStream(inputStream, CompressionMode.Decompress);
		byte[] buffer = new byte[bufferSizeKB * Const.KILOBYTE];

		int bytesRead;

		while ((bytesRead = await compStream.ReadAsync(buffer).ConfigureAwait(false)) > 0) {
			await outStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
		}
	}
}
