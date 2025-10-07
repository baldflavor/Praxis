namespace Praxis;

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

/// <summary>
/// Used for asserting various conditions and throws exceptions when assertions fail
/// </summary>
public static class Assert {
	/// <summary>
	/// Affirms that no exception is thrown during invocation of <paramref name="assertDelegate"/> when passed <paramref name="value"/>.
	/// </summary>
	/// <typeparam name="T">Type of value being affirmed.</typeparam>
	/// <param name="value">Value to affirm.</param>
	/// <param name="assertDelegate">Delegate called with <paramref name="value"/>.</param>
	/// <returns><typeparamref name="T"/></returns>
	/// <exception cref="Exception">May be thrown during invocation of <paramref name="assertDelegate"/>.</exception>
	public static T Affirm<T>(T value, Action<T> assertDelegate) {
		assertDelegate(value);
		return value;
	}

	/// <summary>
	/// Affirms that the value passed to the method evaluates to <c>true</c> against the assertion function.
	/// If it does not, an exception is thrown.
	/// </summary>
	/// <typeparam name="T">Type of value being affirmed.</typeparam>
	/// <param name="value">Value to affirm.</param>
	/// <param name="assertDelegate">Function used for assertion against <paramref name="value"/>.</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Only pass to override.</param>
	/// <returns><typeparamref name="T"/></returns>
	/// <exception cref="Exception">May be thrown during invocation of <paramref name="assertDelegate"/> or if <paramref name="assertDelegate"/> does
	/// not return <c>true</c>.</exception>
	public static T Affirm<T>(T value, Func<T, bool> assertDelegate, [CallerArgumentExpression(nameof(value))] string cae = "") {
		return
				!assertDelegate(value) ?
				throw new Exception("Affirmation of value did not succeed").AddData(new { value, cae }) :
				value;
	}

	/// <summary>
	/// Asserts that the passed argument string contains an expected value using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="arg">Argument to check that contains an expected value</param>
	/// <param name="expected">The expected value that <paramref name="arg"/> must contain</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if the values are not equivalent</exception>
	public static string ContainsOIC(string arg, string expected, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		if (!arg.ContainsOIC(expected))
			throw new ArgumentException($"The passed value did not contain the expected value", nameof(arg)).AddData(new { arg, cae, expected });

		return arg;
	}

	/// <summary>
	/// Asserts that the passed argument string contains an expected value using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="arg">Argument to check that contains an expected value</param>
	/// <param name="expected">The expected value that <paramref name="arg"/> must contain</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if the values are not equivalent</exception>
	public static string ContainsOIC(string arg, char expected, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		if (!arg.Contains(expected, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"The passed value did not contain the expected value", nameof(arg)).AddData(new { arg, cae, expected });

		return arg;
	}

	/// <summary>
	/// Asserts that the passed time span is greater than <see cref="TimeSpan.Zero"/>
	/// </summary>
	/// <param name="arg">Timespan to assert is greater than zero</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if arg is not greater than zero</exception>
	public static TimeSpan GreaterThanZero(TimeSpan arg, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		if (!(arg > TimeSpan.Zero)) {
			throw new ArgumentException("Passed time span was not greater than " + nameof(TimeSpan) + "." + nameof(TimeSpan.Zero), nameof(arg))
					.AddData(new { arg, cae });
		}

		return arg;
	}

	/// <summary>
	/// Asserts that the value passed in <paramref name="arg"/> is an <see cref="StringComparison.OrdinalIgnoreCase"/> match to at least one of the values
	/// passed in via <paramref name="expected"/>
	/// </summary>
	/// <param name="arg">String to check for an equivalent string</param>
	/// <param name="expected">Array of strings as source for checking equivalency</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if there is not a match</exception>
	public static string HasEqualOIC(string arg, IEnumerable<string> expected, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		foreach (string item in expected) {
			if (item.EqualsOIC(arg))
				return arg;
		}

		throw new ArgumentException($"The passed value was not equal to any of the expected values", nameof(arg))
				.AddData(new { arg, cae, expected = string.Join(Const.BROKENVERTBAR, expected) });
	}

	/// <summary>
	/// Asserts that the passed argument string matches an expected value using <see cref="StringComparison.OrdinalIgnoreCase"/>
	/// </summary>
	/// <param name="arg">Argument to check against an expected value</param>
	/// <param name="expected">The expected value that <paramref name="arg"/> must be equivalent to</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if the values are not equivalent</exception>
	public static string IsEqualOIC(string arg, string expected, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		return !arg.EqualsOIC(expected)
				? throw new ArgumentException($"The passed value was not equal to expected value", nameof(arg))
						.AddData(new { arg, cae, expected })
				: arg;
	}

	/// <summary>
	/// Asserts that the passed string returns true against <see cref="Regex.IsMatch(string)"/>
	/// </summary>
	/// <param name="arg">Target to check for a match</param>
	/// <param name="regex">Regular expression to check against</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if the passed string is not a match against the regular expression</exception>
	public static string IsMatch(string arg, Regex regex, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		if (!regex.IsMatch(arg)) {
			throw new ArgumentException("The passed argument did not provide a match against the regular expression", nameof(arg))
					.AddData(new { arg, cae, regex });
		}

		return arg;
	}

	/// <summary>
	/// Asserts that the passed argument is not null
	/// </summary>
	/// <typeparam name="T">Type being checked for nullable value</typeparam>
	/// <param name="arg">Argument being asserted as not <see langword="null"/></param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> indicated as not being null</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="arg"/> is null</exception>
	public static T IsNotNull<T>(this T? arg, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		return
				arg ??
				throw new ArgumentNullException(nameof(arg)).AddData(cae);
	}

	/// <summary>
	/// Asserts that the passed argument is not null
	/// </summary>
	/// <typeparam name="T">Type being checked for nullable value</typeparam>
	/// <param name="arg">Argument being asserted as not <see langword="null"/></param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> indicated as not being null</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="arg"/> is null</exception>
	public static T IsNotNull<T>(this T? arg, [CallerArgumentExpression(nameof(arg))] string cae = "") where T : struct {
		return
				arg ??
				throw new ArgumentNullException(nameof(arg)).AddData(cae);
	}

	/// <summary>
	/// Asserts that the passed string is not null, empty, or contain only whitespace characters
	/// </summary>
	/// <param name="arg">String to assert against</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns>The passed string which is not null or whitespace</returns>
	/// <exception cref="ArgumentNullException"><paramref name="arg"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="arg"/> is empty or consists only of white-space characters.</exception>
	public static string IsNotNullOrWhiteSpace(this string? arg, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		try {
			ArgumentException.ThrowIfNullOrWhiteSpace(arg);
			return arg!;
		}
		catch (Exception ex) {
			ex.AddData(new { arg, cae });
			throw;
		}
	}

	/// <summary>
	/// Performs validation using <see cref="Extension.TryValidate(object, out List{ValidationResult}, string?)"/>.
	/// </summary>
	/// <remarks>
	/// If validation does not pass, an <see cref="AggregateException"/> will be thrown, where each inner exception
	/// will be a validation exception with an attached validation result describing the failure
	/// </remarks>
	/// <param name="arg">Object being validated</param>
	/// <param name="propertyName">If passed, will only validate the specified property of the object rather than the object as a whole.</param>
	/// <returns><paramref name="arg"/></returns>
	/// <exception cref="AggregateException">Thrown if the object is not valid</exception>
	public static T IsValid<T>(this T arg, string? propertyName = null) where T : class {
		bool isValid = arg.TryValidate(out var validationResults, propertyName);

		if (!isValid)
			throw new AggregateException("Validation failed on arg object", [.. validationResults.Select(v => new ValidationException(v, null, null))]);

		return arg;
	}

	/// <summary>
	/// Asserts that the passed <see cref="TimeSpan"/> is not less than <see cref="TimeSpan.Zero"/>
	/// </summary>
	/// <param name="arg">Value to assert is not less than zero</param>
	/// <param name="cae">Caller argument expression captured for logging / attachment to the exception if thrown. Do not pass in this value.</param>
	/// <returns><paramref name="arg"/> if assertion succeeds</returns>
	/// <exception cref="ArgumentException">Thrown if arg is negative</exception>
	public static TimeSpan NonNegative(TimeSpan arg, [CallerArgumentExpression(nameof(arg))] string cae = "") {
		if (arg < TimeSpan.Zero)
			throw new ArgumentException("Passed time span was negative", nameof(arg)).AddData(new { arg, cae });

		return arg;
	}
}
