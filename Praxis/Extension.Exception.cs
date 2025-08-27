namespace Praxis;

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Adds an entry into this exception's .Data dictionary, utilizing caller information for the key.
	/// </summary>
	/// <remarks>
	/// <para>Key utilized is: [Excluding everything before the last <c>\</c> in <paramref name="callerFilePath"/> (if present) and before it's last (potential) <c>.</c>]<c>_</c>[<paramref name="callerMemberName"/>]<c>_</c>[<paramref name="callerArgumentExpression"/>].</para>
	/// <para>As such <b>BE AWARE</b> that a long expression (such as an anonymous object) will end up in a long string name. In this case, <i>pass in <paramref name="callerArgumentExpression"/> with a value to use as the key.</i></para>
	/// <para>If there is an existing entry under this key, then the key will have appended: <c>_</c>[the <b>count of the existing keys <c>+1</c></b>]</para>
	/// Do <b>NOT</b> pass <paramref name="value"/> as a dynamic object as it will cause <paramref name="callerMemberName"/> and <paramref name="callerFilePath"/> to be empty. Cast to an object <c>(object)someDynamicInstance</c> if using dynamics.
	/// <para>As general practice - multiple catch (or .AddData) blocks in the same method can make debugging more difficult. If doing so, make sure the value added is easily traceable.</para>
	/// </remarks>
	/// <param name="ex">Source exception for data dictionary addition.</param>
	/// <param name="value">The value to set. Be cautious about using literal (i.e. <c>3+4</c>, <c>"Simon"</c>) expressions and <b>prefer variables</b>. Also <b>be wary</b> about overly complicated / non-serializable objects. Use <see cref="ToStringProperties{T}(T, string?, string?, string[])"/> if necessary.</param>
	/// <param name="callerArgumentExpression">Argument expression used for key name. Be cautious about using literal (i.e. <c>3+4</c>, <c>"Simon"</c>) expressions and prefer variables.</param>
	/// <param name="callerMemberName">Method caller - used for dictionary key. Filled in by the caller defaultly using compiler services. Pass/override with caution.</param>
	/// <param name="callerFilePath">File path of the caller - used for dictionary key. Filled in by the caller defaultly using compiler services. Pass/override with caution.</param>
	/// <returns><paramref name="ex"/></returns>
	/// <exception cref="Exception">Thrown if the dictionary has reached a maximum size or if an invalid operation is attempted while putting the various key-values into the dictionary.</exception>
	public static Exception AddData(this Exception ex, object? value, [CallerArgumentExpression(nameof(value))] string callerArgumentExpression = "", [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "") {
		Stopwatch sw = Stopwatch.StartNew();
		var cfpSpan = callerFilePath.AsSpan();
		cfpSpan = cfpSpan[(cfpSpan.LastIndexOf('\\') + 1)..];

		int cfpLioSlash = cfpSpan.LastIndexOf('.');
		if (cfpLioSlash != -1)
			cfpSpan = cfpSpan[..cfpLioSlash];

		string dataKey = $"{cfpSpan}_{callerMemberName}";

		lock (ex.Data.SyncRoot) {
			try {
				Dictionary<string, object?>? dkDict = ex.Data[dataKey] as Dictionary<string, object?>;
				if (dkDict is null) {
					dkDict = [];
					ex.Data[dataKey] = dkDict;
				}

				string subKey = _SanitizeCallerArgumentExpression(callerArgumentExpression);
				int existingKeyCount = dkDict.Keys.Where(k => k.StartsWithOIC(subKey)).Count();

				if (existingKeyCount > 0)
					subKey = $"{subKey}_{existingKeyCount + 1}";

				dkDict[subKey] = value;
			}
			catch (ArgumentException ae) {
				ex.Data[dataKey] = $"COULD NOT ADD DATA / ARGUMENT EXCEPTION -> {ae.Message}";
			}
		}

		sw.Stop();
		var elTM = sw.Elapsed.TotalMilliseconds;
		return ex;

		/* ----------------------------------------------------------------------------------------------------------
		 * Sanitizes the passed argument to only contain characters that should be used as keys in
		 * the internal datadictionary */
		static string _SanitizeCallerArgumentExpression(string arg) {
			if (arg.Any(c => char.IsControl(c) || !char.IsAsciiLetterOrDigit(c)))
				return arg.KeepChars(_SanitizeKeepChars);
			else
				return arg;

			static bool _SanitizeKeepChars(char c) {
				return char.IsAsciiLetterOrDigit(c) || c == '_';
			}
		}
	}

	/// <summary>
	/// Returns the name of an exception, and then a list of all public readable properties from the exception
	/// as well as its data dictionary
	/// <para>Contains special handling for System.Data.Entity.Validation.DbEntityValidationException</para>
	/// </summary>
	/// <param name="ex">The exception to retrieve details for</param>
	/// <param name="format">A format string desired for displaying each property on the exception. If null, will use <see cref="ToStringPropertiesFormat"/></param>
	/// <param name="delimiter">The delimiter string to use between each item. If null will use <see cref="ToStringPropertiesDelimiter"/></param>
	/// <returns>A string containing exception details</returns>
	public static string GetDetail(this Exception ex, string? format = default, string? delimiter = default) {
		format ??= ToStringPropertiesFormat;
		delimiter ??= ToStringPropertiesDelimiter;

		Type exType = ex.GetType();

		StringBuilder sb = new(exType.FullName);
		sb.AppendLine();
		sb.Append(ex.ToStringProperties(format, delimiter, "InnerException", "InnerExceptions", "Data", "StackTrace"));
		sb.Append(delimiter);
		sb.AppendFormat(format, "StackTrace", ex.StackTrace);

		/* For: System.Data.Entity.Validation.DbEntityValidationException
		 * The code below uses reflection to pull out relevant validation errors on a model that are thrown by Entity Framework without
		 * needing references System.Data. */
		PropertyInfo? pi = ex.GetType().GetProperty("EntityValidationErrors");
		if (pi != null) {
			sb.Append(delimiter);
			sb.Append("---Validation Errors---");
			sb.Append(delimiter);

			object? entityValidationErrors = pi.GetValue(ex, null);
			PropertyInfo? eveTypePi = null;
			foreach (object? entityValidationResult in (IEnumerable)entityValidationErrors!) {
				eveTypePi ??= entityValidationResult.GetType().GetProperty("ValidationErrors");

				foreach (object? validationError in (IEnumerable)eveTypePi!.GetValue(entityValidationResult)!) {
					sb.Append('[');
					sb.Append(validationError.ToStringProperties(format: format, delimiter: delimiter));
					sb.Append(']');
				}
			}
		}

		// .Data extraction
		if (ex.Data.Count > 0) {
			sb.Append(delimiter);
			StringBuilder dataSb = new();
			dataSb.Append('[');
			dataSb.Append(ex.Data.ToStrings(false, format).Join(delimiter));
			dataSb.Append(']');
			sb.AppendFormat(format, "Data", dataSb.ToString());
		}

		// Aggregate exception specific handling / extraction
		if (exType == typeof(AggregateException)) {
			var aex = (AggregateException)ex;
			for (int i = 0; i < aex.InnerExceptions.Count; i++) {
				sb.Append(delimiter);
				sb.AppendFormat(format, $"Inner exception {i}", aex.InnerExceptions[i].GetDetail(format, delimiter));
			}
		}
		else if (ex.InnerException != null) {
			sb.Append(delimiter);
			sb.AppendFormat(format, "Inner exception", ex.InnerException.GetDetail(format, delimiter));
		}

		return sb.ToString();
	}

	/// <summary>
	/// Returns a value indicating if each value in <see cref="AggregateException.InnerExceptions"/> of <paramref name="arg"/> is of type <see cref="ValidationException"/>
	/// </summary>
	/// <param name="arg">Aggregate Exception to check</param>
	/// <returns>True if all <see cref="AggregateException.InnerExceptions"/> are of type <see cref="ValidationException"/>, otherwise false</returns>
	public static bool HasOnlyValidationExceptions(this AggregateException arg) => arg.HasOnlyValidationExceptions(out _);

	/// <summary>
	/// Returns a value indicating if each value in <see cref="AggregateException.InnerExceptions"/> of <paramref name="arg"/> is of type <see cref="ValidationException"/>
	/// </summary>
	/// <param name="arg">Aggregate Exception to check</param>
	/// <param name="validationStrings">Validation summary text when this exception contains only validation errors</param>
	/// <param name="delimiterMemberName">Used as a delimiter between the member names of a each validation result</param>
	/// <param name="delimiterMessage">Used as a delimiter between the member names and the error message of each validation result</param>
	/// <param name="omitMemberNames">Indicates whether to omit the names of members from each string</param>
	/// <returns>True if all <see cref="AggregateException.InnerExceptions"/> are of type <see cref="ValidationException"/>, otherwise false</returns>
	public static bool HasOnlyValidationExceptions(this AggregateException arg, out IEnumerable<string>? validationStrings, string? delimiterMemberName = default, string? delimiterMessage = default, bool omitMemberNames = false) {
		if (arg.InnerExceptions.All(x => x is ValidationException)) {
			validationStrings = arg.ValidationExceptionResults().ToStrings(delimiterMemberName, delimiterMessage, omitMemberNames);
			return true;
		}
		else {
			validationStrings = null;
			return false;
		}
	}

	/// <summary>
	/// Creates a dictionary of strings and objects of the public instance properties of an exception.
	/// <para><see cref="MethodBase"/> and <see cref="Type"/> properties will have their <c>.ToStrings</c> representation set</para>
	/// <para>This is <strong>not meant for</strong> and <strong>will not work</strong> for two way serialization</para>
	/// </summary>
	/// <param name="ex">Exception to create a dictionary from</param>
	/// <returns>A <see cref="Dictionary{TKey, TValue}"/> if <paramref name="ex"/> is not null</returns>
	[return: NotNullIfNotNull(nameof(ex))]
	public static Dictionary<string, object?>? ToDictionary(this Exception? ex) {
		if (ex is null)
			return null;

		return
				ex.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Select(p => new { pi = p, val = p.GetValue(ex) })
				.ToDictionary(
						e => e.pi.Name,
						v =>
								v.val is null ? null :
								v.pi.PropertyType == typeof(Exception) ? ((Exception)v.val).ToDictionary() :
								v.pi.PropertyType == typeof(MethodBase) ? v.val.ToString() :
								v.pi.PropertyType == typeof(Type) ? v.val.ToString() :
								v.val);
	}

	/// <summary>
	/// Casts all <see cref="AggregateException.InnerExceptions"/> to <see cref="ValidationException"/> objects and returns their <see cref="ValidationResult"/>. Beware of using this
	/// on an instance where not all inner exceptions are of this type
	/// </summary>
	/// <param name="arg">Target exception to use as source</param>
	/// <returns>A <see cref="ValidationResult"/> array</returns>
	public static ValidationResult[] ValidationExceptionResults(this AggregateException arg) => arg.InnerExceptions.Select(ie => ((ValidationException)ie).ValidationResult).ToArray();
}
