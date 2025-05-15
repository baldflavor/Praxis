namespace Praxis.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Class that holds a name and version typically used for holding detail about an assembly
/// </summary>
/// <param name="Name">The name of the assembly</param>
/// <param name="Version">The version of the assembly</param>
public record class NameVersion(string Name, Version Version) {

	/// <summary>
	/// Constant string for when data is unknown
	/// </summary>
	public const string UNKNOWN = nameof(UNKNOWN);

	/// <summary>
	/// Factory method for creating a <see cref="NameVersion"/> instance
	/// </summary>
	/// <param name="arg">Assembly to use for information</param>
	/// <returns><see cref="NameVersion"/> or <see langword="null"/> if <paramref name="arg"/> is <see langword="null"/></returns>
	[return: NotNullIfNotNull(nameof(arg))]
	public static NameVersion? From(AssemblyName? arg) {
		if (arg == null)
			return null;

		return new NameVersion(arg.Name ?? UNKNOWN, arg.Version!);
	}
}