namespace Praxis.Logging;

using System.Runtime.CompilerServices;
using NLog;

/// <summary>
/// Class used for logging messages which includes parameter capture and extra data support.
/// </summary>
/// <remarks>
/// Can be registered in service collections in the following manner:<br/>
/// <c>services.Add{<i>Transient/Scoped/Singleton</i>}(typeof(LogBaseDI&lt;&gt;));</c>
/// </remarks>
/// <typeparam name="T">Type requesting logging instance.</typeparam>
public class LogBaseDI<T>() : LogBase(typeof(T)) {
}
