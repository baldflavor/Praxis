namespace Praxis.IO;

using System.IO.Compression;
using System.Text;

/// <summary>
/// This class is used to serialize and compress objects.   Note that if using the non-XML smash
/// and rebuild methods, that the object will need to have the [Serializable] attribute set to it.
/// </summary>
public static class Compression {

	/// <summary>
	/// UTF8Encoding byte conversion is performed on the passed arg string and then compressed
	/// </summary>
	/// <param name="arg">Target data to compress</param>
	/// <returns>A byte array</returns>
	public static byte[] Compress(string arg) {
		byte[] bytes = Encoding.UTF8.GetBytes(arg);
		return _Compress(bytes);
	}

	/// <summary>
	/// Compresses the passed array of bytes
	/// </summary>
	/// <param name="arg">Target data to compress</param>
	/// <returns>A byte array</returns>
	public static byte[] Compress(byte[] arg) => _Compress(arg);


	/// <summary>
	/// Compresses the passed array of bytes and returns it as a base64 string
	/// </summary>
	/// <param name="arg">Target data to compress</param>
	/// <returns>A base64 string</returns>
	public static string CompressToBase64(byte[] arg) => Convert.ToBase64String(_Compress(arg));


	/// <summary>
	/// UTF8Encoding byte conversion is performed on the passed arg string and then compressed and returned as a base64 string
	/// </summary>
	/// <param name="arg">Target data to compress</param>
	/// <returns>A base64 string</returns>
	public static string CompressToBase64(string arg) {
		byte[] bytes = Encoding.UTF8.GetBytes(arg);
		return Convert.ToBase64String(_Compress(bytes));
	}



	/// <summary>
	/// Decompresses the source data into an array of bytes
	/// </summary>
	/// <param name="source">Source data to decompress</param>
	/// <returns>A decompressed byte array</returns>
	public static byte[] Decompress(byte[] source) => _Decompress(source);


	/// <summary>
	/// Decompresses the source base64 string data into an array of bytes
	/// </summary>
	/// <param name="source">Source base64 string data to decompress</param>
	/// <returns>A decompressed byte array</returns>
	public static byte[] Decompress(string source) => _Decompress(Convert.FromBase64String(source));


	/// <summary>
	/// Decompresses the source data into a utf8 encoded string
	/// </summary>
	/// <param name="source">Source data to decompress</param>
	/// <returns>A decompressed string</returns>
	public static string DecompressToString(byte[] source) => Encoding.UTF8.GetString(_Decompress(source));


	/// <summary>
	/// Decompresses the source base64 string data into a utf8 encoded string
	/// </summary>
	/// <param name="source">Source base64 string data to decompress</param>
	/// <returns>A decompressed string</returns>
	public static string DecompressToString(string source) => Encoding.UTF8.GetString(_Decompress(Convert.FromBase64String(source)));



	/// <summary>
	/// Compresses an array of bytes using deflate compression
	/// </summary>
	/// <param name="bytes">The bytes to compress</param>
	/// <returns>A byte array</returns>
	private static byte[] _Compress(byte[] bytes) {
		using MemoryStream ms = new();
		using DeflateStream ds = new(ms, CompressionLevel.Optimal);
		ds.Write(bytes, 0, bytes.Length);
		ds.Flush();
		ds.Close();
		return ms.ToArray();
	}

	/// <summary>
	/// Decompresses the passed byte array
	/// </summary>
	/// <param name="source">The source array of bytes</param>
	/// <returns>Uncompressed bytes</returns>
	private static byte[] _Decompress(byte[] source) {
		byte[] toReturn;

		using MemoryStream outer = new(source);
		outer.Position = 0;

		using DeflateStream ds = new(outer, CompressionMode.Decompress);
		ds.Flush();

		using MemoryStream inner = new();
		ds.CopyTo(inner);
		toReturn = inner.ToArray();

		return toReturn;
	}
}