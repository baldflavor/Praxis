namespace Praxis.Logging;

using System.Runtime.CompilerServices;
using NLog;

/// <summary>
/// Class used as a base for working with logging operations as part of dependency injected scenarios
/// </summary>
public class LogBaseDI<T> {

	/// <summary>
	/// Holds a reference to an <see cref="Logger"/> used for writing log entries
	/// </summary>
	/// <remarks>
	/// This instance should be used for internal logging operations as it may already have properties set
	/// </remarks>
	private readonly Logger _logger = LogManager.GetLogger(typeof(T).FullName);


	/// <summary>
	/// Used for capturing an exception and writing it out to appropriate storage. Calls <see cref="OnLoggedError(Exception)"/> post entry.
	/// </summary>
	/// <remarks>
	/// Do <b>NOT</b> pass <paramref name="data"/> as a dynamic object as it will cause <paramref name="callerMemberName"/> and <paramref name="callerFilePath"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.
	/// </remarks>
	/// <param name="exception">The <see cref="Exception"/> to be logged</param>
	/// <param name="data">Supplemental data properties to log. Do <b>NOT</b> pass as a dynamic object as it will cause <paramref name="callerMemberName"/> to not function. Cast to an object <c>(object)dynamicInstance</c> if using dynamics.</param>
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedError(Exception)"/></exception>
	public void Error(Exception exception, object? data = null, [CallerMemberName] string callerMemberName = "") {
		AddProperties(data, _logger)
				.WithProperty(Constant.CALLER, callerMemberName)
				.Error(exception);

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
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="message"/> does not have a cogent value</exception>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedInfo(string)"/></exception>
	public void Info(string message, object? data = null, [CallerMemberName] string callerMemberName = "") {
		ArgumentException.ThrowIfNullOrWhiteSpace(message);

		AddProperties(data, _logger)
				.WithProperty(Constant.CALLER, callerMemberName)
				.Info(message);

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
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="ArgumentException">Thrown if both <paramref name="exception"/> and <paramref name="message"/> are null</exception>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed, or if one is thrown by code in <see cref="OnLoggedWarn(Exception?, string?)"/></exception>
	public void Warn(Exception? exception = null, string? message = null, object? data = null, [CallerMemberName] string callerMemberName = "") {
		if (exception == default && string.IsNullOrWhiteSpace(message))
			throw new ArgumentException("Must have either an exception or message to log");

		AddProperties(data, _logger)
				.WithProperty(Constant.CALLER, callerMemberName)
				.Warn(exception, message);

		OnLoggedWarn(exception, message);
	}



	/// <summary>
	/// Used for adding properties to a <see cref="Logger"/> before performing a log operation
	/// </summary>
	/// <remarks>
	/// Base level code performs the following:
	/// <list type="number">
	///	<item>If <paramref name="data"/> is <see langword="null"/>, <c>returns</c></item>
	///	<item>If <paramref name="data"/> is <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>
	///	of <see cref="string"/>, <see cref="object"/>, adds each to the <paramref name="logger"/>, then <c>returns</c></item>
	///	<item>Adds <paramref name="data"/> to a property named <see cref="Constant.DATA"/></item>
	/// </list>
	/// </remarks>
	/// <param name="data">Data used for <paramref name="logger"/> properties</param>
	/// <param name="logger">The logger that is being used for logging purposes</param>
	/// <returns><see cref="Logger"/></returns>
	protected virtual Logger AddProperties(object? data, Logger logger) {
		if (data == null)
			return logger;
		else if (data is IEnumerable<KeyValuePair<string, object>> dDict)
			return logger.WithProperties(dDict);
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
}
