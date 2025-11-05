namespace Praxis.Logging;

using System.Runtime.CompilerServices;
using NLog;

/// <summary>
/// Extensions for NLog / Logger operations
/// </summary>
public static class Extension {
	/// <summary>
	/// Logs a message indicating initialization. This is most useful as the first entry added when the application domain is starting
	/// </summary>
	/// <param name="logger">Logger to use for writing out initialization message</param>
	/// <param name="initInfo">Information about the domain where the application started</param>
	/// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
	/// <exception cref="Exception">Thrown if the logging operation could not be completed</exception>
	public static void Initialized(this Logger logger, InitializeInfo initInfo, [CallerMemberName] string callerMemberName = "") {
		logger
				.WithProperty(nameof(InitializeInfo), initInfo)
				.WithProperty(Constant.CALLER, callerMemberName)
				.Info(Constant.INITIALIZED);
	}

	/// <summary>
	/// Using an ID from an NLog entry, returns numerical components.
	/// </summary>
	/// <remarks>
	/// Argument should be in the form of:<br/>"[double]_[int]"
	/// <para>Typically created from an NLog layout using: <c>$"{DateTime.UtcNow.ToOADate()}_${{sequenceid}}"</c>.</para>
	/// Example: <c>45965.950441388886_133</c>
	/// </remarks>
	/// <param name="idKit">Combination ID as created by an NLog layout.</param>
	/// <returns>Tuple of: double that represents an OADate (batch), and the sequence underneath the batch.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the index of the separator character is less than 1.</exception>
	/// <exception cref="FormatException">Thrown if portions of the passed value cannot be parsed into respective numeric values.</exception>
	public static (double oadBatch, int sequence) ToNLogIDComponents(this string idKit) {
		try {
			int undIndex = idKit.IndexOf('_');
			ArgumentOutOfRangeException.ThrowIfLessThan(undIndex, 1);

			return (
				double.Parse(idKit.AsSpan(0, undIndex)),
				int.Parse(idKit.AsSpan(undIndex + 1))
			);
		}
		catch (Exception ex) {
			ex.Data[nameof(idKit)] = idKit;
			throw;
		}
	}
}
