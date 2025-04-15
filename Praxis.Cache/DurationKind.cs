namespace Praxis.Cache;

/// <summary>
/// Specifies how the duration factor should be used when determining time spans through the implicit TimeSpan operator or otherwise
/// </summary>
public enum DurationKind {

	/// <summary>
	/// Hour specified for time spans
	/// </summary>
	Hours,

	/// <summary>
	/// Minute specifier for time spans
	/// </summary>
	Minutes,

	/// <summary>
	/// Second specifier for time spans
	/// </summary>
	Seconds
}