namespace Praxis.Crypto;

using System.Security.Cryptography;

/// <summary>
/// Used for generating and working with token values
/// </summary>
public static class Token {

	/// <summary>
	/// Generates a cryptographically random string sequence which is suitable for use in basic auth
	/// </summary>
	/// <param name="length">The length of the resultant string to generate</param>
	/// <param name="alphaNumericOnly">Whether or not the returned token should be alphanumeric characters only</param>
	/// <returns>A randomly generated string</returns>
	public static string Generate(int length, bool alphaNumericOnly = true) {
		ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);

		int[] restricted;

		int lbound;
		int ubound;
		if (alphaNumericOnly) {
			restricted = [58, 59, 60, 61, 62, 63, 64, 91, 92, 93, 94, 95, 96];
			lbound = 48;
			ubound = 123;
		}
		else {
			restricted = [58, 34, 39, 92];
			lbound = 33;
			ubound = 127;
		}

		int idx = 0;
		char[] choices = new char[ubound - lbound - restricted.Length];
		for (int i = lbound; i < ubound; i++) {
			if (restricted.Contains(i))
				continue;
			choices[idx++] = (char)i;
		}

		return new string(RandomNumberGenerator.GetItems<char>(choices, length));
	}

	/// <summary>
	/// Returns a new Guid as a string without - delimiters
	/// </summary>
	/// <returns>A string</returns>
	public static string GuidAlpha() => Guid.NewGuid().ToString("N");
}