namespace Praxis;

using System.Numerics;
using System.Text;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Compares two byte arrays for equivalency
	/// </summary>
	/// <param name="argA">The first arg array</param>
	/// <param name="argB">The second arg array</param>
	/// <returns>True if equivalent, otherwise false</returns>
	public static bool IsEqualTo(this byte[] argA, byte[] argB) {
		int argALength = argA.Length;
		if (argALength != argB.Length)
			return false;

		// Check vector chunks for equivalency
		int totalProcessed = 0;
		int vectorLen = Vector<byte>.Count;
		while (totalProcessed < argALength) {
			if (!Vector.EqualsAll(new Vector<byte>(argA, totalProcessed), new Vector<byte>(argB, totalProcessed)))
				return false;

			totalProcessed += vectorLen;
		}

		return true;
	}

	/// <summary>
	/// Returns whether this byte is implicitly equal to <see cref="Const.TRUEINT"/>
	/// </summary>
	/// <param name="arg">arg byte to check</param>
	/// <returns>True or false</returns>
	public static bool IsTrue(this byte arg) => arg == Const.TRUEINT;

	/// <summary>
	/// Returns a Base64 encoded string from the passed byte array
	/// </summary>
	/// <param name="arg">Byte values used for conversion.</param>
	/// <param name="options">Formatting options used when composing output.</param>
	/// <returns>string</returns>
	public static string ToBase64String(this byte[] arg, Base64FormattingOptions options = Base64FormattingOptions.None) => Convert.ToBase64String(arg, options);

	/// <summary>
	/// Returns the hexadecimal string representation of a byte array
	/// </summary>
	/// <param name="arg">The arg array of bytes</param>
	/// <returns>string</returns>
	public static string ToHexString(this byte[] arg) => string.Join("", arg.Select(b => b.ToString("X2")));

	/// <summary>
	/// Encodes an array of bytes into a (uri) token safe string
	/// </summary>
	/// <param name="arg">Byte array to encode</param>
	/// <returns>An encoded string which is safe for use as a uri (querystring) value token</returns>
	public static string TokenEncode(this byte[] arg) {
		if (arg.Length == 0)
			return "";

		string b64 = Convert.ToBase64String(arg);
		int eqCount = b64.Count(c => c == '=');
		int newCharLen = b64.Length - eqCount;
		char[] newChars = new char[newCharLen + 1];

		for (int i = 0; i < newCharLen; i++) {
			char c = b64[i];
			newChars[i] = c switch {
				'=' => throw new Exception("Found an = sign in a position that wasn't supposed to have one"),
				'+' => '-',
				'/' => '_',
				_ => c,
			};
		}

		newChars[newCharLen] = (char)('0' + eqCount);

		return new string(newChars);
	}

	/// <summary>
	/// Returns a UTF-8 encoded string from the passed UTF-8 byte array
	/// </summary>
	/// <param name="arg">Byte array composing a UTF-8 string</param>
	/// <returns>string</returns>
	public static string ToUTF8String(this byte[] arg) => Encoding.UTF8.GetString(arg);
}
