namespace Praxis;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Returns the number of digits in an <c>int</c>
	/// </summary>
	/// <remarks>
	/// Utilizes an <c>if</c> chain - which in .NET (Core) has been determined to be faster than using the math formula:
	/// <c>floor(log10(x)+1)</c>
	/// </remarks>
	/// <param name="arg">Value to look for number of digits</param>
	/// <returns>The number of digits</returns>
	public static int NumDigits(this int arg) {
		if (arg >= 0) {
			if (arg < 10)
				return 1;
			if (arg < 100)
				return 2;
			if (arg < 1000)
				return 3;
			if (arg < 10000)
				return 4;
			if (arg < 100000)
				return 5;
			if (arg < 1000000)
				return 6;
			if (arg < 10000000)
				return 7;
			if (arg < 100000000)
				return 8;
			if (arg < 1000000000)
				return 9;
			return 10;
		}
		else {
			if (arg > -10)
				return 2;
			if (arg > -100)
				return 3;
			if (arg > -1000)
				return 4;
			if (arg > -10000)
				return 5;
			if (arg > -100000)
				return 6;
			if (arg > -1000000)
				return 7;
			if (arg > -10000000)
				return 8;
			if (arg > -100000000)
				return 9;
			if (arg > -1000000000)
				return 10;
			return 11;
		}
	}

	/// <summary>
	/// Returns the number of digits in an <c>long</c>
	/// </summary>
	/// <remarks>
	/// Utilizes an <c>if</c> chain - which in .NET (Core) has been determined to be faster than using the math formula:
	/// <c>floor(log10(x)+1)</c>
	/// </remarks>
	/// <param name="arg">Value to look for number of digits</param>
	/// <returns>The number of digits</returns>
	public static int NumDigits(this long arg) {
		if (arg >= 0) {
			if (arg < 10L)
				return 1;
			if (arg < 100L)
				return 2;
			if (arg < 1000L)
				return 3;
			if (arg < 10000L)
				return 4;
			if (arg < 100000L)
				return 5;
			if (arg < 1000000L)
				return 6;
			if (arg < 10000000L)
				return 7;
			if (arg < 100000000L)
				return 8;
			if (arg < 1000000000L)
				return 9;
			if (arg < 10000000000L)
				return 10;
			if (arg < 100000000000L)
				return 11;
			if (arg < 1000000000000L)
				return 12;
			if (arg < 10000000000000L)
				return 13;
			if (arg < 100000000000000L)
				return 14;
			if (arg < 1000000000000000L)
				return 15;
			if (arg < 10000000000000000L)
				return 16;
			if (arg < 100000000000000000L)
				return 17;
			if (arg < 1000000000000000000L)
				return 18;
			return 19;
		}
		else {
			if (arg > -10L)
				return 2;
			if (arg > -100L)
				return 3;
			if (arg > -1000L)
				return 4;
			if (arg > -10000L)
				return 5;
			if (arg > -100000L)
				return 6;
			if (arg > -1000000L)
				return 7;
			if (arg > -10000000L)
				return 8;
			if (arg > -100000000L)
				return 9;
			if (arg > -1000000000L)
				return 10;
			if (arg > -10000000000L)
				return 11;
			if (arg > -100000000000L)
				return 12;
			if (arg > -1000000000000L)
				return 13;
			if (arg > -10000000000000L)
				return 14;
			if (arg > -100000000000000L)
				return 15;
			if (arg > -1000000000000000L)
				return 16;
			if (arg > -10000000000000000L)
				return 17;
			if (arg > -100000000000000000L)
				return 18;
			if (arg > -1000000000000000000L)
				return 19;
			return 20;
		}
	}
}
