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
		StringBuilder sb = new StringBuilder(arg[0]);

		for (int i = 1; i < arg.Length; i++) {
			char currentChar = arg[i];

			if (char.IsUpper(currentChar))
				sb.Append(' ');

			sb.Append(currentChar);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Determines if a string contains another string via <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="source">Source string to search</param>
	/// <param name="arg">source string to check for containment in <paramref name="source"/></param>
	/// <returns>True if the source is contained in source, otherwise false</returns>
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
	/// <param name="arg">source string to end-search on <paramref name="source"/></param>
	/// <returns>True if <paramref name="source"/> ends with <paramref name="arg"/>, otherwise false</returns>
	public static bool EndsWithOIC(this string? source, string? arg) {
		if (source is null || arg is null)
			return false;

		return source.EndsWith(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Returns whether two strings are equal using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// <para>Null-safe</para>
	/// </summary>
	/// <param name="source">Source string to use for comparison</param>
	/// <param name="arg">source string to compare to</param>
	/// <returns>True if the strings are case insensitively equal using an ordinal comparison</returns>
	public static bool EqualsOIC(this string? source, string? arg) {
		if (source is null)
			return arg is null;

		return source.Equals(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Formats a 10 digit string? into a North American (e.g. (xxx) xxx-xxxx) format
	/// <para>source string is run through a process to strip all non-alphanumeric characters</para>
	/// <para>Will return the original string if formatting cannot be applied</para>
	/// </summary>
	/// <param name="arg">The string to format</param>
	/// <returns>A string</returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? Format10DigitPhoneNumber(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return arg;

		Match re = Validation.USPhone().Match(StripNonDigit(arg)!.ToUpperInvariant());

		//Look for 4 groups because there is always one included as the whole entire match
		if (!re.Success || re.Groups.Count < 4)
			return arg;

		return $"({re.Groups["area"].Value}) {re.Groups["prefix"].Value}-{re.Groups["suffix"].Value}";
	}

	/// <summary>
	/// Formats the passed North America E164 phone number to <c>(###) ###-####</c>.
	/// </summary>
	/// <remarks>
	/// Note that if the source string is invalid (either because of digit count, included characters, etc -- then the original string may be returned).
	/// </remarks>
	/// <param name="arg">Value to format</param>
	/// <returns><c>string</c></returns>
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
	/// <param name="arg">source string to locate index of</param>
	/// <returns>-1 nulls are present, or if the <paramref name="arg"/> is not found in <paramref name="source"/>; otherwise the index of</returns>
	public static int IndexOfOIC(this string? source, string? arg) {
		if (source is null || arg is null)
			return -1;

		return source.IndexOf(arg, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Inserts a source string at the specified interval in a string
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
	/// Returns whether or not the source string is a certain number of numeric digits
	/// </summary>
	/// <param name="arg">source string to check</param>
	/// <param name="numberOfDigits">The exact number of digits that must be present; if null then any number of digits (only) are valid</param>
	/// <returns>Boolean value indicating whether the source string is the correct number of digits</returns>
	public static bool IsDigits(this string? arg, int? numberOfDigits = null) {
		if (string.IsNullOrWhiteSpace(arg))
			return false;

		return
				Regex.IsMatch(
				arg,
				numberOfDigits is null ? Validation.DIGIT : $@"^\d{{{numberOfDigits}}}$");
	}

	/// <summary>
	/// Determines whether this string can be parsed into a numeric type, specifically using an attempt to parse the string into a double.
	/// </summary>
	/// <param name="arg">The string to check.</param>
	/// <param name="numVal">Numeric value (as a double) resultant from the numeric check.</param>
	/// <returns><c>true</c> if numeric, otherwise <c>false</c>.</returns>
	public static bool IsNumeric(this string? arg, out double numVal) {
		if (string.IsNullOrWhiteSpace(arg)) {
			numVal = double.MinValue;
			return false;
		}

		return double.TryParse(arg, out numVal);
	}

	/// <summary>
	/// Determines whether this string can be parsed into a numeric type, specifically using an attempt to parse the string into a double.
	/// </summary>
	/// <param name="arg">The string to check.</param>
	/// <returns><c>true</c> if numeric, otherwise <c>false</c>.</returns>
	public static bool IsNumeric(this string? arg) => arg.IsNumeric(out _);

	/// <summary>
	/// Returns whether or not the source string is valid as an E.164 phone number. An optional "required length" can be passed.
	/// </summary>
	/// <param name="arg">The source string to check</param>
	/// <param name="requiredLength">Total required length of the string including the + prefix / country code</param>
	/// <returns>True if valid, otherwise false</returns>
	public static bool IsValidE164Number(this string? arg, int? requiredLength = 12) {
		return
				!string.IsNullOrWhiteSpace(arg) &&
				(requiredLength is null || arg.Length == requiredLength) &&
				Validation.E164Phone().IsMatch(arg);
	}

	/// <summary>
	/// Uses System.Net.Mail.MailAddress instantiation to catch a FormatException and thus determine whether an email address
	/// is valid given .NET's mail sending mechanisms.
	/// <para>Is quite liberal</para>
	/// <para>You may wish to use this instead of or in conjunction with the Email regular expressions in this class.</para>
	/// </summary>
	/// <param name="arg">source string to check</param>
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
	/// <param name="arg">source string to validate</param>
	/// <returns>True if valid otherwise false</returns>
	public static bool IsValidHypertextUrl(this string? arg) {
		return Uri.TryCreate(arg, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
	}

	/// <summary>
	/// Returns whether the passed string can be parsed into an IP 4 or IP 6 address. For IP 4 three . (quads) must be present.
	/// <para>There is no other special consideration for IP6 addresses other than that they succeed IPAddress.Parse()</para>
	/// </summary>
	/// <param name="arg">source string to work with</param>
	/// <param name="ipEndpoint">Resultant <see cref="System.Net.IPEndPoint"/> if valid.</param>
	/// <returns><c>True</c> if valid, otherwise <c>false</c>.</returns>
	public static bool IsValidIPEndpoint(this string? arg, out System.Net.IPEndPoint? ipEndpoint) {
		bool successful = System.Net.IPEndPoint.TryParse(arg!, out System.Net.IPEndPoint? res);
		ipEndpoint = res;
		return successful;
	}

	/// <summary>
	/// Returns a <c>string</c> composed of characters where <paramref name="cEval"/> returns <c>true</c> for each in <paramref name="source"/>.
	/// </summary>
	/// <remarks>
	/// Depending on length of source, will use stack allocated span or heap creation.
	/// </remarks>
	/// <param name="source">Source to keep characters from.</param>
	/// <param name="cEval">Function used to evaluate whether a <c>char</c> should be part of the returned value.</param>
	/// <returns><c>string</c></returns>
	public static string KeepChars(this string source, Func<char, bool> cEval) {
		// The maximum length of a string when determing whether to use stack allocation or heap allocation
		const int _MAXSTACKSIZE = 256;

		return source.Length <= _MAXSTACKSIZE ? _StackAlloc() : _Heap();

		/* ----------------------------------------------------------------------------------------------------------
		 * Performs replacement using stack allocation */
		string _StackAlloc() {
			Span<char> buffer = stackalloc char[source.Length];
			int count = 0;

			foreach (char c in source) {
				if (cEval(c))
					buffer[count++] = c;
			}

			return new string(buffer[..count]);
		}

		/* ----------------------------------------------------------------------------------------------------------
		 * Performs replacement using the heap / string.Create */
		string _Heap() {
			int newLength = 0;
			foreach (char c in source) {
				if (cEval(c))
					newLength++;
			}

			return string.Create(newLength, source, (span, source) => {
				int i = 0;
				foreach (char c in source) {
					if (cEval(c))
						span[i++] = c;
				}
			});
		}
	}

	/// <summary>
	/// Returns a string with characters masked from the left side.
	/// </summary>
	/// <param name="source"><c>string</c> to mask.</param>
	/// <param name="maskCount">The number of characters to mask from the left.</param>
	/// <param name="maskChar">The character to use as the masking character.</param>
	/// <returns><c>string</c></returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maskCount"/> is less than <c>1</c>.</exception>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? Mask(this string? source, int maskCount, char maskChar) {
		ArgumentOutOfRangeException.ThrowIfLessThan(maskCount, 1);

		if (source is null)
			return null;

		if (source.Length == 0)
			return source;

		if (maskCount < 1)
			return source;

		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < source.Length; i++) {
			if (i < maskCount)
				sb.Append(maskChar);
			else
				sb.Append(source[i]);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Removes all occurances of a <c>char</c> from a source <c>string</c>.
	/// </summary>
	/// <param name="source">Source for which to perform replacement.</param>
	/// <param name="toRemove"><c>char</c> to remove from <paramref name="source"/>.</param>
	/// <returns><c>string</c></returns>
	public static string RemoveChar(this string source, char toRemove) => KeepChars(source, c => c != toRemove);

	/// <summary>
	/// Removes all occurances of matching <c>char</c> from a source <c>string</c>.
	/// </summary>
	/// <param name="source">Source for which to perform replacement.</param>
	/// <param name="charsToRemove"><c>char</c> to remove from <paramref name="source"/>.</param>
	/// <returns><c>string</c></returns>
	public static string RemoveChars(this string source, ReadOnlySpan<char> charsToRemove) {
		var removeSet = new HashSet<char>([.. charsToRemove]);

		return KeepChars(source, c => !removeSet.Contains(c));
	}

	/// <summary>
	/// Returns whether one string starts with another using <see cref="StringComparison.OrdinalIgnoreCase"/>.
	/// </summary>
	/// <param name="source">Source string.</param>
	/// <param name="check">String to check against <paramref name="source"/>.</param>
	/// <returns><c>true</c> if <paramref name="source"/> starts with <paramref name="check"/>, otherwise <c>false</c>.</returns>
	public static bool StartsWithOIC(this string? source, string? check) {
		if (source is null || check is null)
			return false;

		return source.StartsWith(check, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Strips all characters except for: <c>(0-9, a-z, A-Z)</c>.
	/// </summary>
	/// <param name="arg">Source value.</param>
	/// <returns><c>string?</c></returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? StripNonAlphaNumeric(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return arg;

		if (arg.Any(c => !char.IsAsciiLetterOrDigit(c)))
			return KeepChars(arg, char.IsAsciiLetterOrDigit);
		else
			return arg;
	}

	/// <summary>
	/// Strips all characters except for: <c>(0-9)</c>.
	/// </summary>
	/// <param name="arg">Source value.</param>
	/// <returns><c>string?</c></returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static string? StripNonDigit(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return arg;

		if (arg.Any(c => !char.IsAsciiDigit(c)))
			return KeepChars(arg, char.IsAsciiDigit);
		else
			return arg;
	}

	/// <summary>
	/// Gets a substring of charcters that exists between two other strings.
	/// </summary>
	/// <param name="source">Source of substring.</param>
	/// <param name="startAt">Start string for bounding of substring operation.</param>
	/// <param name="endAt">End string for bounding of substring operation.</param>
	/// <param name="comparisonKind"><c>StringComparison</c> for index check of <paramref name="startAt"/> and <paramref name="endAt"/>.</param>
	/// <returns>If neither <paramref name="startAt"/> nor <paramref name="endAt"/> can be located, null will be returned. Otherwise, a substring between.</returns>
	public static string? Substring(this string? source, string? startAt, string? endAt, StringComparison comparisonKind = StringComparison.OrdinalIgnoreCase) {
		if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(startAt) || string.IsNullOrWhiteSpace(endAt))
			return source;

		int firstIdx = source.IndexOf(startAt, comparisonKind);
		if (firstIdx == -1)
			return null;

		firstIdx += startAt.Length;

		int secondIdx = source.IndexOf(endAt, firstIdx, comparisonKind);
		if (secondIdx == -1)
			return null;

		return source[firstIdx..secondIdx];
	}

	/// <summary>
	/// Returns a substring consisting of the portion after the first index of another.
	/// </summary>
	/// <remarks>
	/// If <paramref name="after"/> is not found in <paramref name="source"/> an exception will be thrown.
	/// </remarks>
	/// <param name="source">Source string.</param>
	/// <param name="after">String after which the substring will be performed.</param>
	/// <param name="stringComparison">Comparison to use when searching for <paramref name="after"/> in <paramref name="source"/>.</param>
	/// <returns><c>string</c></returns>
	/// <exception cref="Exception">Thrown if <paramref name="after"/> is not found in <paramref name="source"/></exception>
	public static string SubstringAfter(this string source, string after, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) {
		int index = source.IndexOf(after, stringComparison);

		if (index == -1)
			throw new Exception("Source string does not contain the after portion").AddData(source).AddData(after).AddData(stringComparison);

		return source[(index + after.Length)..];
	}

	/// <summary>
	/// Gets a substring from the left side portion of a string using the passed number of characters.
	/// </summary>
	/// <remarks>
	/// Handles cases where the requested number of characters is greater than the length of <paramref name="source"/>. 
	/// </remarks>
	/// <param name="source">Source string.</param>
	/// <param name="charCount">Number of characters to return from <paramref name="source"/>.</param>
	/// <returns><c>string</c></returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? SubstringLeft(this string? source, int charCount) {
		if (string.IsNullOrWhiteSpace(source) || charCount < 1)
			return null;

		if (charCount >= source.Length)
			return source;

		return source[..charCount];
	}

	/// <summary>
	/// Decodes a <see cref="TokenEncode(byte[])"/> encoded string back into an array of source bytes.
	/// </summary>
	/// <param name="arg">String to decode.</param>
	/// <returns><c>byte[]</c></returns>
	public static byte[] TokenDecode(this string arg) {
		if (arg is null)
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
	/// Executes <see cref="Extension.TokenEncode(byte[])"/> on the UTF-8 bytes of a source string.
	/// </summary>
	/// <param name="arg">String to encode.</param>
	/// <returns>A (url) token safe <c>string</c>.</returns>
	public static string TokenEncode(this string arg) => arg.ToUTF8Bytes()!.TokenEncode();

	/// <summary>
	/// Returns the source string title cased.
	/// </summary>
	/// <remarks>
	/// Note in title case rules, text in call caps (e.g. DOG) will not be title cased. Words like McDonald's will also not properly title case.
	/// </remarks>
	/// <param name="source">Value to title case.</param>
	/// <param name="cultureInfo">Cculture info to use when title casing; if null then the current culture of the executing thread will be used.</param>
	/// <returns><c>string</c></returns>
	public static string ToTitleCase(this string source, System.Globalization.CultureInfo? cultureInfo = null) => (cultureInfo ?? System.Globalization.CultureInfo.CurrentCulture).TextInfo.ToTitleCase(source);

	/// <summary>
	/// Using the passed string, get it's representation as a UTF-8 byte array.
	/// </summary>
	/// <param name="arg">Source value.</param>
	/// <returns>Binary data representation of <paramref name="arg"/> in UTF-8 encoding.</returns>
	public static byte[] ToUTF8Bytes(this string arg) => Encoding.UTF8.GetBytes(arg);

	/// <summary>
	/// Trims a string and returns <c>null</c> if it contains only whitespace characters.
	/// </summary>
	/// <param name="arg">Source value.</param>
	/// <returns><c>string</c></returns>
	public static string? TrimToNull(this string? arg) {
		if (string.IsNullOrWhiteSpace(arg))
			return null;

		return arg.Trim();
	}

	/// <summary>
	/// Truncates a string so that it fits into the passed max length, and attempts to keep whole words rather than truncating them in the middle.
	/// </summary>
	/// <remarks>
	/// Very short strings or strings where the length is less than the max length may get returned without a trailing ellipsis.
	/// </remarks>
	/// <param name="arg">Source value.</param>
	/// <param name="maxLength">Maximum length of string to be returned</param>
	/// <returns><c>string</c></returns>
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
	/// Accepts a value that may or may not have the +1 prefix or otherwise contrains superflous (e.g. (, -) characters needed for a valid E164 phone number,
	/// and prefixes it with either/both a + or a 1. If <paramref name="source"/> would not constitute a valid E164 formatted number, then this method
	/// will return a subsequently invalid value that is not suitable for dialing.
	/// </summary>
	/// <param name="source">Source value.</param>
	/// <param name="autoStripAndTrim">Indicates whether to auto strip and trim the supplied value.</param>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? TryMakeNAE164PhoneNumber(this string? source, bool autoStripAndTrim = true) {
		if (string.IsNullOrWhiteSpace(source))
			return null;

		StringBuilder sb;

		if (autoStripAndTrim)
			sb = new(source.StripNonDigit()!.Trim());
		else
			sb = new(source);

		if (sb.Length == 10)
			sb.Insert(0, '1');

		if (sb[0] != '+')
			sb.Insert(0, '+');

		return sb.ToString();
	}
}
