namespace Praxis.Logging;

using System.Runtime.CompilerServices;
using NLog;

/// <summary>
/// Class used for working with various logging facilities
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Logging" /> class
/// </remarks>
/// <param name="attachProperties">Each of these are added to the root <see cref="_logger"/> and will thus be added to every entry that
/// is added through subsequent log entries. This can cause logs to be larger than necessary in size.</param>
public class LogBase(Type type) {
	/// <summary>
	/// Holds a reference to an <see cref="Logger"/> used for writing log entries
	/// </summary>
	/// <remarks>
	/// This instance should be used for internal logging operations as it may already have properties set
	/// </remarks>
	private readonly Logger _logger = LogManager.GetLogger(type.FullName);

	/// <summary>
	/// Used for capturing an exception and writing it out to appropriate storage. Calls <see cref="OnLoggedError(Exception)"/> post entry.
	/// </summary>
	/// <remarks>
	/// Do <b>NOT</b> pass <paramref name="data"/> as a dynamic object as it will cause <paramref name="callerMemberName"/> and <paramref name="callerFilePath"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.
	/// </remarks>
	/// <param name="exception">The <see cref="Exception"/> to be logged</param>
	/// <param name="data">Supplemental data properties to log. Do <b>NOT</b> pass as a dynamic object as it will cause <paramref name="callerMemberName"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.</param>
	/// <param name="callerFilePath">File path of the caller - used in combination with <paramref name="callerMemberName"/> for location hint. Filled in by the caller defaultly using compiler services</param>
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedError(Exception)"/></exception>
	public void Error(Exception exception, object? data = null, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "") {
		AddProperties(data, _logger.WithProperty(Constant.CALLER, _CallerInfo(callerFilePath, callerMemberName))).Error(exception);
		OnLoggedError(exception);
	}

	/// <summary>
	/// Logs an informational message. Calls <see cref="OnLoggedInfo(string)"/> post entry.
	/// </summary>
	/// <remarks>
	/// Do <b>NOT</b> pass <paramref name="data"/> as a dynamic object as it will cause <paramref name="callerMemberName"/> and <paramref name="callerFilePath"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.
	/// </remarks>
	/// <param name="message">Message to be logged</param>
	/// <param name="data">Supplemental data properties to log. Do <b>NOT</b> pass as a dynamic object as it will cause <paramref name="callerMemberName"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.</param>
	/// <param name="callerFilePath">File path of the caller - used in combination with <paramref name="callerMemberName"/> for location hint. Filled in by the caller defaultly using compiler services</param>
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="message"/> does not have a cogent value</exception>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedInfo(string)"/></exception>
	public void Info(string message, object? data = null, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "") {
		ArgumentException.ThrowIfNullOrWhiteSpace(message);
		AddProperties(data, _logger.WithProperty(Constant.CALLER, _CallerInfo(callerFilePath, callerMemberName))).Info(message);
		OnLoggedInfo(message);
	}

	/// <summary>
	/// Used for capturing a warning and writing it out to appropriate storage. Calls <see cref="OnLoggedWarn(Exception?, string?)"/> post entry.
	/// </summary>
	/// <remarks>
	/// Do <b>NOT</b> pass <paramref name="data"/> as a dynamic object as it will cause <paramref name="callerMemberName"/> and <paramref name="callerFilePath"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.
	/// </remarks>
	/// <param name="exception">The <see cref="Exception"/> to be logged</param>
	/// <param name="message">Alternative messaging to log alongside <paramref name="exception"/></param>
	/// <param name="data">Supplemental data properties to log. Do <b>NOT</b> pass as a dynamic object as it will cause <paramref name="callerMemberName"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.</param>
	/// <param name="callerFilePath">File path of the caller - used in combination with <paramref name="callerMemberName"/> for location hint. Filled in by the caller defaultly using compiler services</param>
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="ArgumentException">Thrown if both <paramref name="exception"/> and <paramref name="message"/> are null</exception>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedWarn(Exception?, string?)"/></exception>
	public void Warn(Exception? exception = null, string? message = null, object? data = null, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "") {
		if (exception == default && string.IsNullOrWhiteSpace(message))
			throw new ArgumentException("Must have either an exception or message to log");

		AddProperties(data, _logger.WithProperty(Constant.CALLER, _CallerInfo(callerFilePath, callerMemberName))).Warn(exception, message);
		OnLoggedWarn(exception, message);
	}


	/// <summary>
	/// Used for adding properties to a <see cref="Logger"/> before performing a log operation
	/// </summary>
	/// <remarks>Base level code adds <paramref name="data"/> to a property named <see cref="DATA"/> if it is not <see langword="null"/></remarks>
	/// <param name="data">Data to be added as a property to the logger</param>
	/// <param name="logger">The logger that is being used for logging purposes</param>
	/// <returns><see cref="Logger"/></returns>
	protected virtual Logger AddProperties(object? data, Logger logger) {
		if (data == null)
			return logger;
		else
			return logger.WithProperty(Constant.DATA, data);
	}

	/// <summary>
	/// Code that runs after all other operations in <see cref="Error(Exception, object?, string?)"/>
	/// </summary>
	/// <remarks>Base code performs no action</remarks>
	/// <param name="exception"><see cref="Exception"/> that was logged</param>
	protected virtual void OnLoggedError(Exception exception) {
	}

	/// <summary>
	/// Code that runs after all other operations in <see cref="Info(string, object?, string?)"/>
	/// </summary>
	/// <remarks>Base code performs no action</remarks>
	/// <param name="message">Message that was logged as informational</param>
	protected virtual void OnLoggedInfo(string message) {
	}

	/// <summary>
	/// Code that runs after all other operations in <see cref="Warn(Exception?, string?, object?, string?)"/>
	/// </summary>
	/// <remarks>
	/// Base code performs no action
	/// <para>At least one of the passed parameters will not be <see langword="null"/></para>
	/// </remarks>
	/// <param name="exception">Excepion that may have been logged</param>
	/// <param name="message">Message that may have been logged</param>
	protected virtual void OnLoggedWarn(Exception? exception, string? message) {
	}


	/// <summary>
	/// Uses caller last portion of the file path and member name to create a single string for location hint
	/// </summary>
	/// <param name="callerFilePath">File path of the caller passed from one of the categorical log methods</param>
	/// <param name="callerMemberName">Member name of the caller passed from one of the categorical log methods</param>
	/// <returns><see cref="string"/></returns>
	private static string _CallerInfo(ReadOnlySpan<char> callerFilePath, ReadOnlySpan<char> callerMemberName) {
		callerFilePath = callerFilePath[(callerFilePath.LastIndexOf('\\') + 1)..];
		int lIndex = callerFilePath.LastIndexOf('.');
		if (lIndex != -1)
			callerFilePath = callerFilePath[..lIndex];

		Span<char> combined = new char[callerFilePath.Length + callerMemberName.Length + 1];
		callerFilePath.CopyTo(combined);
		combined[callerFilePath.Length] = '_';
		callerMemberName.CopyTo(combined[(callerFilePath.Length + 1)..]);
		return combined.ToString();
	}
}
