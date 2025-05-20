namespace Praxis.Logging;

using System.Runtime.CompilerServices;
using NLog;

/// <summary>
/// Extensions for NLog / Logger operations
/// </summary>
public static class Extension
{
    /// <summary>
    /// Logs a message indicating initialization. This is most useful as the first entry added when the application domain is starting
    /// </summary>
    /// <param name="logger">Logger to use for writing out initialization message</param>
    /// <param name="initInfo">Information about the domain where the application started</param>
    /// <param name="callerMemberName">Captures the name of the method where this method was called. Filled in by the caller defaultly using compiler services</param>
    /// <exception cref="Exception">Thrown if the logging operation could not be completed</exception>
    public static void Initialized(this Logger logger, InitializeInfo initInfo, [CallerMemberName] string callerMemberName = "")
    {
        logger
            .WithProperty(nameof(InitializeInfo), initInfo)
            .WithProperty(Constant.CALLER, callerMemberName)
            .Info(Constant.INITIALIZED);
    }
}
