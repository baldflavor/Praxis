namespace Praxis;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Adds a space before each uppercase character in a string -- excluding the first character.
	/// If multiple uppercase characters are present a space will be added between each of them.
	/// <example>
	/// <code>
	/// MyTargetString
	/// returns
	/// My Target String
	///
	/// MyAPIAddress
	/// returns
	/// My A P I Address
	/// </code>
	/// </example>
	/// </summary>
	/// <param name="arg">String to use for source</param>
	/// <returns>A string with spaces added</returns>
	public static string AddSpacesBeforeUppercase(this string arg) {
		List<char> output = [];
		output.Add(arg[0]);

		for (int i = 1; i < arg.Length; i++) {
			char currentChar = arg[i];

			if (char.IsUpper(currentChar))
				output.Add(' ');

			output.Add(currentChar);
		}

		return string.Join("", output);
	}

	/// <summary>
	/// Determines if a string contains another string via <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="source">Source string to search</param>
	/// <param name="arg">arg string to check for containment in <paramref name="source"/></param>
	/// <returns>True if the arg is contained in source, otherwise false</returns>
	public static bool ContainsOIC(this string source, string arg) => IndexOfOIC(source, arg) != -1;

	/// <summary>
	/// Deserializes a string into a strong type using <see cref="System.Text.Json.JsonSerializer"/> with <see cref="Json.Options"/>
	/// </summary>
	/// <typeparam name="T">The type to deserialize to</typeparam>
	/// <param name="json">Valid Json string</param>
	/// <returns><typeparamref name="T"/></returns>
	public static T? Deserialize<T>(this string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json, Json.Options);

	/// <summary>
	/// Returns whether one string ends with another using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="source">Source string</param>
	/// <param name="arg">arg string to end-search on <paramref name="source"/></param>
	/// <returns>True if <paramref name="source"/> ends with <paramref name="arg"/>, otherwise false</returns>
	public static bool EndsWithOIC(this string? source, string? arg) {
		if (source == null || arg == null)
			return false;

		return source.EndsWith(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Returns whether two strings are equal using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// <para>Null-safe</para>
	/// </summary>
	/// <param name="source">Source string to use for comparison</param>
	/// <param name="arg">arg string to compare to</param>
	/// <returns>True if the strings are case insensitively equal using an ordinal comparison</returns>
	public static bool EqualsOIC(this string? source, string? arg) {
		if (source == null)
			return arg == null;

		return source.Equals(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Formats a 10 digit string? into a North American (e.g. (xxx) xxx-xxxx) format
	/// <para>arg string is run through a process to strip all non-alphanumeric characters</para>
	/// <para>Will return the original string if formatting cannot be applied</para>
	/// </summary>
	/// <param name="arg">The string to format</param>
	/// <returns>A string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? Format10DigitPhoneNumber(this string? arg) {
		if (string.IsNullOrEmpty(arg))
			return arg;

		Match re = Validation.USPhone().Match(StripNonDigit(arg)!.ToUpperInvariant());

		//Look for 4 groups because there is always one included as the whole entire match
		if (!re.Success || re.Groups.Count < 4)
			return arg;

		return $"({re.Groups["area"].Value}) {re.Groups["prefix"].Value}-{re.Groups["suffix"].Value}";
	}

	/// <summary>
	/// Formats the passed string as N.A. valid E164 formatted phone number. Note that if the source string is invalid (either because
	/// of digit count, included characters, etc -- then the original string may be returned)
	/// </summary>
	/// <param name="arg">Value to format</param>
	/// <returns>An E164 valid phone number for N.A.</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? FormatNAE164PhoneNumber(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return null;

		if (arg.Length != 12)
			return arg;

		return arg[2..].Format10DigitPhoneNumber();
	}

	/// <summary>
	/// Returns the index of a string within another using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="source">Source string</param>
	/// <param name="arg">arg string to locate index of</param>
	/// <returns>-1 nulls are present, or if the <paramref name="arg"/> is not found in <paramref name="source"/>; otherwise the index of</returns>
	public static int IndexOfOIC(this string? source, string? arg) {
		if (source == null || arg == null)
			return -1;

		return source.IndexOf(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Inserts a arg string at the specified interval in a string
	/// <para>The default insertion is good for breaking HTML strings for wrapping</para>
	/// </summary>
	/// <param name="arg">The string to use as a base for inserting</param>
	/// <param name="interval">How many characters between breaks</param>
	/// <param name="toInsert">The string to insert on interval</param>
	/// <returns>A string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? InsertAtInterval(this string? arg, int interval, string? toInsert = "&shy;") {
		if (string.IsNullOrWhiteSpace(arg) || string.IsNullOrEmpty(toInsert) || interval < 1)
			return arg;

		StringBuilder sb = new();

		int counter = 1;
		int len = arg.Length;
		for (int i = 0; i < len; i++) {
			sb.Append(arg[i]);
			if (counter == interval) {
				sb.Append(toInsert);
				counter = 1;
			}
			else {
				++counter;
			}
		}

		return sb.ToString();
	}

	/// <summary>
	/// Returns whether or not the arg string is a certain number of numeric digits
	/// </summary>
	/// <param name="arg">arg string to check</param>
	/// <param name="numberOfDigits">The exact number of digits that must be present; if null then any number of digits (only) are valid</param>
	/// <returns>Boolean value indicating whether the arg string is the correct number of digits</returns>
	public static bool IsDigits(this string? arg, int? numberOfDigits = null) {
		if (string.IsNullOrWhiteSpace(arg))
			return false;

		return
				Regex.IsMatch(
				arg,
				numberOfDigits == null ? Validation.DIGIT : $@"^\d{{{numberOfDigits}}}$");
	}

	/// <summary>
	/// Determines whether this string can be parsed into a numeric type, specifically using an attempt to parse the string into a double
	/// </summary>
	/// <param name="arg">The string to check</param>
	/// <returns>A boolean value indicating numeric validity</returns>
	public static bool IsNumeric(this string? arg) {
		if (string.IsNullOrEmpty(arg))
			return false;

		return double.TryParse(arg, out double _);
	}

	/// <summary>
	/// Returns whether or not the arg string is valid as an E.164 phone number. An optional "required length" can be passed.
	/// </summary>
	/// <param name="arg">The arg string to check</param>
	/// <param name="requiredLength">Total required length of the string including the + prefix / country code</param>
	/// <returns>True if valid, otherwise false</returns>
	public static bool IsValidE164Number(this string? arg, int? requiredLength = 12) {
		return
				!string.IsNullOrWhiteSpace(arg) &&
				(requiredLength == null || arg.Length == requiredLength) &&
				Validation.E164Phone().IsMatch(arg);
	}

	/// <summary>
	/// Uses System.Net.Mail.MailAddress instantiation to catch a FormatException and thus determine whether an email address
	/// is valid given .NET's mail sending mechanisms.
	/// <para>Is quite liberal</para>
	/// <para>You may wish to use this instead of or in conjunction with the Email regular expressions in this class.</para>
	/// </summary>
	/// <param name="arg">arg string to check</param>
	/// <returns>True if it is, false if it is not</returns>
	public static bool IsValidEmailAddress(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return false;

		try {
			_ = new System.Net.Mail.MailAddress(arg);
			return true;
		}
		catch (FormatException) {
			return false;
		}
	}

	/// <summary>
	/// Returns whether the passed string is a valid absolute http(s) url
	/// </summary>
	/// <param name="arg">arg string to validate</param>
	/// <returns>True if valid otherwise false</returns>
	public static bool IsValidHypertextUrl(this string? arg) {
		return Uri.TryCreate(arg, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
	}

	/// <summary>
	/// Returns whether the passed string can be parsed into an IP 4 or IP 6 address. For IP 4 three . (quads) must be present.
	/// <para>There is no other special consideration for IP6 addresses other than that they succeed IPAddress.Parse()</para>
	/// </summary>
	/// <param name="arg">arg string to work with</param>
	/// <returns>True if valid, otherwise false</returns>
	public static bool IsValidIPEndpoint(this string? arg, out System.Net.IPEndPoint? ipEndpoint) {
		bool successful = System.Net.IPEndPoint.TryParse(arg!, out System.Net.IPEndPoint? res);
		ipEndpoint = res;
		return successful;
	}

	/// <summary>
	/// Returns the string with characters masked/replaced on the left side
	/// </summary>
	/// <param name="arg">The string to mask</param>
	/// <param name="maskCount">The number of characters to mask from the left</param>
	/// <param name="maskChar">The character to use as the masking character</param>
	/// <returns>A string (empty possible) or null (if arg is null)</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? Mask(this string? arg, int maskCount, char maskChar) {
		if (arg == null)
			return null;

		if (arg == "" || arg.Length < 1)
			return "";

		if (maskCount < 1)
			return arg;

		string mask = new(maskChar, maskCount);

		if (maskCount >= arg.Length)
			return mask;

		return $"{mask}{arg[maskCount..]}";
	}

	/// <summary>
	/// Returns whether one string starts with another using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="source">Source string</param>
	/// <param name="arg">arg string to start-search on <paramref name="source"/></param>
	/// <returns>True if <paramref name="source"/> starts with <paramref name="arg"/>, otherwise false</returns>
	public static bool StartsWithOIC(this string? source, string? arg) {
		if (source == null || arg == null)
			return false;

		return source.StartsWith(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Strips all non alphanumeric characters (0-9, a-z, A-Z) from a string
	/// </summary>
	/// <param name="arg">The arg string to use</param>
	/// <returns>The stripped string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? StripNonAlphaNumeric(this string? arg) {
		if (string.IsNullOrEmpty(arg))
			return arg;

		return _StripNonAlphanumeric().Replace(arg, "");
	}

	/// <summary>
	/// Strips all non digit (0-9) characters from a string
	/// </summary>
	/// <param name="arg">The arg string</param>
	/// <returns>The stripped string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? StripNonDigit(this string? arg) {
		if (string.IsNullOrEmpty(arg))
			return arg;

		return _StripNonDigit().Replace(arg, "");
	}

	/// <summary>
	/// Gets a arg substring where it is between two other strings
	/// </summary>
	/// <param name="arg">String to use as source of substring</param>
	/// <param name="startAt">String fragment to use as start of substring</param>
	/// <param name="endAt">String at which point to end the substring</param>
	/// <param name="comparisonKind">The string comparison type to use</param>
	/// <returns>If neither <paramref name="startAt"/> nor <paramref name="endAt"/> can be located, null will be returned. Otherwise, a substring between the searched parameters</returns>
	public static string? Substring(this string? arg, string? startAt, string? endAt, StringComparison comparisonKind = StringComparison.OrdinalIgnoreCase) {
		if (string.IsNullOrWhiteSpace(arg) || string.IsNullOrWhiteSpace(startAt) || string.IsNullOrWhiteSpace(endAt))
			return arg;

		int firstIdx = arg.IndexOf(startAt, comparisonKind);
		if (firstIdx == -1)
			return null;

		firstIdx += startAt.Length;

		int secondIdx = arg.IndexOf(endAt, firstIdx, comparisonKind);
		if (secondIdx == -1)
			return null;

		return arg[firstIdx..secondIdx];
	}

	/// <summary>
	/// Returns a substring consisting of the portion after the first index of another
	/// </summary>
	/// <remarks>
	/// If <paramref name="after"/> is not found in <paramref name="arg"/> an exception will be thrown
	/// </remarks>
	/// <param name="arg">Source string to use</param>
	/// <param name="after">The string after which the substring will be performed</param>
	/// <param name="stringComparison">Comparison to use when searching for <paramref name="after"/> in <paramref name="arg"/></param>
	/// <returns><see cref="string"/></returns>
	/// <exception cref="Exception">Thrown if <paramref name="after"/> is not found in <paramref name="arg"/></exception>
	public static string SubstringAfter(this string arg, string after, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) {
		int index = arg.IndexOf(after, stringComparison);

		if (index == -1)
			throw new Exception("Source string does not contain the after portion").AddData(new { arg, after, stringComparison });

		return arg[(index + after.Length)..];
	}

	/// <summary>
	/// Gets a substring from the left side portion of a string using the passed number of characters.
	/// If <paramref name="charCount"/> is greater than the length of the string, then <paramref name="arg"/> is returned
	/// </summary>
	/// <param name="arg">The arg string to use</param>
	/// <param name="charCount">The number of characters to return</param>
	/// <returns><see cref="string"/></returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? SubstringLeft(this string? arg, int charCount) {
		if (arg == null)
			return null;

		if (arg.Length == 0 || charCount < 1)
			return "";

		if (charCount >= arg.Length)
			return arg;

		return arg[..charCount];
	}

	/// <summary>
	/// Decodes a <see cref="TokenEncode(byte[])"/> encoded string back into an array of source bytes
	/// </summary>
	/// <param name="arg">String to decode</param>
	/// <returns>An array of bytes</returns>
	public static byte[] TokenDecode(this string arg) {
		if (arg == null)
			throw new ArgumentException("Cannot be null", nameof(arg));

		int sourceLen = arg.Length;
		if (sourceLen == 0)
			return [];

		int eqCount = arg[sourceLen - 1] - '0';

		char[] newChars = new char[sourceLen - 1 + eqCount];
		int newCharLen = newChars.Length;

		int loopLen = sourceLen - 1;
		int i;
		for (i = 0; i < loopLen; i++) {
			char c = arg[i];
			newChars[i] = c switch {
				'=' => throw new Exception("Found an = sign in a position that wasn't supposed to have one"),
				'-' => '+',
				'_' => '/',
				_ => c,
			};
		}

		for (; i < newCharLen; i++) {
			newChars[i] = '=';
		}

		return Convert.FromBase64CharArray(newChars, 0, newCharLen);
	}

	/// <summary>
	/// Executes the extension method of byte.TokenEncode on the arg string by first getting the UTF-8 bytes of the arg string
	/// </summary>
	/// <param name="arg">String to encode</param>
	/// <returns>a (url) token safe string</returns>
	public static string TokenEncode(this string arg) => arg.ToUTF8Bytes()!.TokenEncode();

	/// <summary>
	/// Returns the arg string title cased. Note in title case rules, text in call caps (e.g. DOG) will not be title cased. Words like McDonald's will also not properly title case
	/// </summary>
	/// <param name="arg">arg string to title case</param>
	/// <param name="cultureInfo">A arg culture info to use when title casing; if null then the current culture of the executing thread will be used</param>
	/// <returns>A title cased string</returns>
	public static string ToTitleCase(this string arg, System.Globalization.CultureInfo? cultureInfo = null) => (cultureInfo ?? System.Globalization.CultureInfo.CurrentCulture).TextInfo.ToTitleCase(arg);

	/// <summary>
	/// Returns the result of Encoding.UTF8.GetBytes(arg)
	/// </summary>
	/// <param name="arg"></param>
	/// <returns>A byte array</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static byte[]? ToUTF8Bytes(this string? arg) => arg == null ? null : Encoding.UTF8.GetBytes(arg);

	/// <summary>
	/// If the arg string <see cref="string.IsNullOrWhiteSpace(string?)"/> is true, then null is returned. Otherwise a trimmed version of the string is returned
	/// </summary>
	/// <param name="arg">The arg string to trim / null</param>
	/// <returns>Either a trimmed string or null</returns>
	public static string? TrimToNull(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return null;

		return arg.Trim();
	}

	/// <summary>
	/// Truncates a arg string so that it fits into the passed max length and attempts to keep whole words rather than truncating them in the middle
	/// <para>Very short strings or strings where the length is less than the max length may get returned without a trailing ellipsis</para>
	/// </summary>
	/// <param name="arg">arg string to work with</param>
	/// <param name="maxLength">Maximum length of string to be returned</param>
	/// <returns>A string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? TruncateOnWord(this string? arg, int maxLength) {
		if (string.IsNullOrWhiteSpace(arg) || arg.Length <= maxLength)
			return arg;

		const string INDICATOR = "...";
		int indicatorLength = INDICATOR.Length;

		string[] words = arg.Split(' ');
		int wordsLength = words.Length;

		if (words.Length == 1) {
			if (arg.Length - maxLength - indicatorLength > 0)
				return string.Concat(arg.AsSpan(0, maxLength - indicatorLength), INDICATOR);
			else
				return arg[..maxLength];
		}
		else if (words[0].Length + indicatorLength >= maxLength) {
			return arg[..(maxLength - indicatorLength)] + INDICATOR;
		}

		StringBuilder sb = new();
		for (int i = 0; i < wordsLength; i++) {
			if (sb.Length + words[i].Length + indicatorLength >= maxLength)
				break;
			sb.Append(words[i]);
			sb.Append(' ');
		}

		sb.Remove(sb.Length - 1, 1);
		sb.Append(INDICATOR);

		return sb.ToString();
	}

	/// <summary>
	/// Accepts a string? that may or may not have the +1 prefix or otherwise contrains superflous (e.g. (, -) characters needed for a valid E164 phone number
	/// and prefixes it with either/both a + or a 1. If the supplied <paramref name="arg"/> would not constitute a valid E164 formatted number, then this method
	/// will return a subsequently invalid value that is not suitable for dialing
	/// </summary>
	/// <param name="arg">Phone number string to set</param>
	/// <param name="autoStripAndTrim">Indicates whether to auto strip and trim the supplied phone field</param>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? TryMakeNAE164PhoneNumber(this string? arg, bool autoStripAndTrim = true) {
		if (string.IsNullOrWhiteSpace(arg))
			return null;

		StringBuilder sb;

		if (autoStripAndTrim)
			sb = new(arg.StripNonDigit()!.Trim());
		else
			sb = new(arg);

		if (sb.Length == 10)
			sb.Insert(0, '1');

		if (sb[0] != '+')
			sb.Insert(0, '+');

		return sb.ToString();
	}

	[GeneratedRegex("[\\W_]+")]
	private static partial Regex _StripNonAlphanumeric();

	[GeneratedRegex("\\D+")]
	private static partial Regex _StripNonDigit();
}