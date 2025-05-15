namespace Praxis.Logging;

using NLog;
using NLog.Layouts;
using NLog.Targets;

/// <summary>
/// Class used for configuring NLog for layouts and outputs.
/// </summary>
/// <remarks>
/// <para>When using the configure methods for Json output, a particular point of note is the <c>LogID</c>. This is initialized as: <c>DateTime.UtcNow.ToOADate()_${sequenceid}</c>.</para>
/// <para>This means that every logged entry past that point will all have, essentially, a batch prefix of the current UTC OADate, combined with a rolling sequence that starts at <c>1</c>.</para>
/// <para>Thus you can find all logs that belong to a specific application domain "<i>run</i>" using the OADate portion that comes before the _</para>
/// <para>It is recommended that immediately after setup on call the <see cref="LogBase.Initialize(InitializeInfo, string, string)"/> method to record machine information as the start of a log batch</para>
/// The static constructor of this class sets up and configures some global options in NLog.
/// <list type="bullet">
/// <item>TimeSource is globally set to FastUtcTimeSource</item>
/// </list>
/// </remarks>
public static class NLogConfiguration {

	/// <summary>
	/// Static constructor, sets global and base level properties for working with NLog
	/// </summary>
	static NLogConfiguration() {
		NLog.Time.TimeSource.Current = new NLog.Time.FastUtcTimeSource(); // Fast time source with UTC stamp updated every 15ms
	}


	/// <summary>
	/// Configures NLog for Json formatted file output
	/// </summary>
	/// <param name="directoryName">The directory where NLog should write logs</param>
	/// <param name="includeLoggerName">Indicates whether to include the logger "name" as part of the layout</param>
	/// <param name="maxArchiveFiles">Maximum number of archive files to be kept before they are deleted. Files are archived according to <see cref="FileArchivePeriod.Day"/></param>
	/// <param name="recursionLimit">Maximum number of properties to use for recursion when writing out objects as Json</param>
	/// <param name="tzi"><see cref="TimeZoneInfo"/> used for log entries for converting the time stamp to local time when necessary. If <see langword="null"/> then <see cref="TimeZoneInfo.Local"/> will be used</param>
	public static void ConfigureForJsonFileOutput(string directoryName, bool includeLoggerName, int maxArchiveFiles = 183, int recursionLimit = 5, TimeZoneInfo? tzi = null) {
		_SetLogManagerConfiguration(_GetFileTarget(directoryName, _GetJsonLayout(includeLoggerName, recursionLimit, tzi ?? TimeZoneInfo.Local), maxArchiveFiles));
	}

	/// <summary>
	/// Configures NLog for Json formatted file and memory output
	/// </summary>
	/// <param name="directoryName">The directory where NLog should write logs</param>
	/// <param name="includeLoggerName">Indicates whether to include the logger "name" as part of the layout</param>
	/// <param name="maxArchiveFiles">Maximum number of archive files to be kept before they are deleted. Files are archived according to <see cref="FileArchivePeriod.Day"/></param>
	/// <param name="maxLogCount">Maximum number of logs to keep in memory</param>
	/// <param name="recursionLimit">Maximum number of properties to use for recursion when writing out objects as Json</param>
	/// <param name="tzi"><see cref="TimeZoneInfo"/> used for log entries for converting the time stamp to local time when necessary. If <see langword="null"/> then <see cref="TimeZoneInfo.Local"/> will be used</param>
	/// <returns><see cref="MemoryTarget"/></returns>
	public static MemoryTarget ConfigureForJsonFileOutputAndMemoryTarget(string directoryName, bool includeLoggerName, int maxArchiveFiles = 183, int maxLogCount = 10_000, int recursionLimit = 5, TimeZoneInfo? tzi = null) {
		var layout = _GetJsonLayout(includeLoggerName, recursionLimit, tzi ?? TimeZoneInfo.Local);
		var memoryTarget = _GetMemoryTarget(layout, maxLogCount);
		_SetLogManagerConfiguration(memoryTarget, _GetFileTarget(directoryName, layout, maxArchiveFiles));
		return memoryTarget;
	}

	/// <summary>
	/// Configures NLog for Json formatted memory output
	/// </summary>
	/// <param name="includeLoggerName">Indicates whether to include the logger "name" as part of the layout</param>
	/// <param name="maxLogCount">Maximum number of logs to keep in memory</param>
	/// <param name="recursionLimit">Maximum number of properties to use for recursion when writing out objects as Json</param>
	/// <param name="tzi"><see cref="TimeZoneInfo"/> used for log entries for converting the time stamp to local time when necessary. If <see langword="null"/> then <see cref="TimeZoneInfo.Local"/> will be used</param>
	/// <returns><see cref="MemoryTarget"/></returns>
	public static MemoryTarget ConfigureForJsonMemoryTarget(bool includeLoggerName, int maxLogCount = 10_000, int recursionLimit = 5, TimeZoneInfo? tzi = null) {
		var target = _GetMemoryTarget(_GetJsonLayout(includeLoggerName, recursionLimit, tzi ?? TimeZoneInfo.Local), maxLogCount);
		_SetLogManagerConfiguration(target);
		return target;
	}

	/// <summary>
	/// Gets a file target configured for writing out log files to disk
	/// </summary>
	/// <param name="directoryName">The directory where NLog should write logs</param>
	/// <param name="layout">The layout to be used for formatting log entries</param>
	/// <param name="maxArchiveFiles">Maximum number of archive files to be kept before they are deleted. Files are archived according to <see cref="FileArchivePeriod.Day"/></param>
	/// <returns><see cref="FileTarget"/></returns>
	private static FileTarget _GetFileTarget(string directoryName, Layout layout, int maxArchiveFiles) {
		string nlogForwardSlashPath = directoryName.Replace('\\', '/');
		nlogForwardSlashPath = nlogForwardSlashPath[..(nlogForwardSlashPath.EndsWith('/') ? nlogForwardSlashPath.Length - 1 : nlogForwardSlashPath.Length)];

		return new FileTarget("jsonFileTarget") {
			ArchiveEvery = FileArchivePeriod.Day,
			ArchiveFileName = $"{nlogForwardSlashPath}/archive/${{shortdate}}_{{###}}.zip",
			EnableArchiveFileCompression = true,
			FileName = $"{nlogForwardSlashPath}/${{shortdate}}.json",
			FileNameKind = FilePathKind.Absolute,
			MaxArchiveFiles = maxArchiveFiles,
			Layout = layout
		};
	}

	/// <summary>
	/// Gets a layout configured for json formatting
	/// </summary>
	/// <param name="includeLoggerName">Indicates whether to include the logger "name" as part of the layout</param>
	/// <param name="recursionLimit">Maximum number of properties to use for recursion when writing out objects as Json</param>
	/// <param name="tzi"><see cref="TimeZoneInfo"/> used for log entries for converting the time stamp to local time when necessary.</param>
	/// <returns><see cref="JsonLayout"/></returns>
	private static JsonLayout _GetJsonLayout(bool includeLoggerName, int recursionLimit, TimeZoneInfo tzi) {
		var jsLayout = new JsonLayout {
			ExcludeEmptyProperties = true,
			MaxRecursionLimit = recursionLimit,
			RenderEmptyObject = false,
			SuppressSpaces = true
		};

		jsLayout.Attributes.Add(new JsonAttribute("ID", Layout.FromString($"{DateTime.UtcNow.ToOADate()}_${{sequenceid}}")));
		jsLayout.Attributes.Add(new JsonAttribute("Level", Layout.FromString("${level:format=FirstCharacter}")));
		jsLayout.Attributes.Add(new JsonAttribute("Utc", Layout.FromString("${longdate}")));
		jsLayout.Attributes.Add(new JsonAttribute("Tzi", tzi.Id));

		if (includeLoggerName)
			jsLayout.Attributes.Add(new JsonAttribute("Logger", Layout.FromString("${logger}")));

		jsLayout.Attributes.Add(new JsonAttribute("Message", Layout.FromString("${message}")));
		jsLayout.Attributes.Add(
			new JsonAttribute(
				"Properties",
				new JsonLayout {
					ExcludeEmptyProperties = true,
					IncludeEventProperties = true,
					MaxRecursionLimit = recursionLimit,
					RenderEmptyObject = false,
					SuppressSpaces = true
				}) {
				Encode = false
			});
		jsLayout.Attributes.Add(new JsonAttribute("Exceptions", Layout.FromString("${exception:format=@}")) { Encode = false });

		return jsLayout;
	}

	/// <summary>
	/// Gets a memory target configured for storing logs in memory
	/// </summary>
	/// <param name="layout">The layout to be used for formatting log entries</param>
	/// <param name="maxLogCount">Maximum number of logs to keep in memory</param>
	/// <returns><see cref="MemoryTarget"/></returns>
	private static MemoryTarget _GetMemoryTarget(Layout layout, int maxLogCount) {
		return new MemoryTarget("jsonMemoryTarget") {
			Layout = layout,
			MaxLogsCount = maxLogCount
		};
	}

	/// <summary>
	/// Sets the <see cref="LogManager.Configuration"/> to a new configuration with the passed targets added with rules for log levels
	/// </summary>
	/// <param name="targets">Targets to add to a new configuration to be set</param>
	private static void _SetLogManagerConfiguration(params Target[] targets) {
		var config = new NLog.Config.LoggingConfiguration();

		foreach (var target in targets)
			config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);

		LogManager.Configuration = config;
	}
}