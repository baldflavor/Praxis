namespace Praxis;

using System.Text;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Removes whitespace characters by back walking from the end of the string builder until the first non-whitespace character is encountered.
	/// </summary>
	/// <remarks>
	/// Character walking a string builder by index <b>can</b> be very slow due to 'chunking'.
	/// <para>Further information: <see href="https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder.chars?view=net-9.0#system-text-stringbuilder-chars(system-int32)"/></para>
	/// </remarks>
	/// <param name="sb">Instance being altered.</param>
	/// <returns><see cref="StringBuilder"/></returns>
	public static StringBuilder TrimEnd(this StringBuilder sb) {
		for (int i = sb.Length - 1; i >= 0; i--) {
			char c = sb[i];
			if (char.IsWhiteSpace(c))
				sb.Remove(i, 1);
			else
				break;
		}

		return sb;
	}
}
