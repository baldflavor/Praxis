namespace Praxis.IO;

using System.Numerics;
using System.Reflection;

/// <summary>
/// Tool methods for working with file/io related tasks
/// </summary>
public static class Tools {

	/// <summary>
	/// Returns a value indicating whether two files on disk are "equal" on a binary basis.
	/// <para>Both files must exist</para>
	/// </summary>
	/// <param name="pathA">The full file path to the first file</param>
	/// <param name="pathB">The full file path to the second file</param>
	/// <returns>True if the files are equal, false if they are not</returns>
	/// <exception cref="ArgumentException"></exception>
	public static async Task<bool> FilesEqual(string pathA, string pathB) {
		const int CHUNKSIZE = 131072;  // 4096 * 32

		if (!File.Exists(pathA))
			throw new ArgumentException($"File does not exist [{pathA}]", nameof(pathA));

		if (!File.Exists(pathB))
			throw new ArgumentException($"File does not exist [{pathB}]", nameof(pathB));

		// If the same file is referenced return true
		if (pathA.EqualsOIC(pathB))
			return true;

		// Check the lengths of the files - if different, they are not the same
		FileInfo fiA = new(pathA);
		FileInfo fiB = new(pathB);
		if (fiA.Length != fiB.Length)
			return false;

		//Check via buffers/vectors
		byte[] bufferA = new byte[CHUNKSIZE];
		byte[] bufferB = new byte[CHUNKSIZE];

		using FileStream fsA = fiA.OpenRead();
		using FileStream fsB = fiB.OpenRead();

		while (true) {
			int readA = await fsA.ReadAtLeastAsync(bufferA, CHUNKSIZE, false).ConfigureAwait(false);
			int readB = await fsB.ReadAtLeastAsync(bufferB, CHUNKSIZE, false).ConfigureAwait(false);

			// The end of the streams have been reached without triggering a mismatch so the
			// files are the same
			if (readA == 0)
				return true;

			// Check vector chunks from the read binary buffers and determine equivalency
			int totalProcessed = 0;
			int vectorLen = Vector<byte>.Count;
			while (totalProcessed < bufferA.Length) {
				if (!Vector.EqualsAll(new Vector<byte>(bufferA, totalProcessed), new Vector<byte>(bufferB, totalProcessed)))
					return false;

				totalProcessed += vectorLen;
			}
		}
	}
}