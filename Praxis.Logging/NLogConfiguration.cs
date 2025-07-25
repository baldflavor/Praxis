namespace Praxis.Logging;

using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

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
	/// <remarks>
	/// Also adds object transformation for common framework types that typically either cannot be serialized or produce
	/// recursive / overly long serialized information. (i.e. <see cref="Assembly"/>, <see cref="Type"/>, <see cref="FileSystemInfo"/>
	/// </remarks>
	static NLogConfiguration() {
		NLog.Time.TimeSource.Current = new NLog.Time.FastUtcTimeSource(); // Fast time source with UTC stamp updated every 15ms

		LogManager.Setup().SetupSerialization((s) => {
			s.RegisterObjectTransformation<Assembly>(e => {
				AssemblyName assemblyName = e.GetName();
				return new { assemblyName.FullName, assemblyName.Version };
			});

			s.RegisterObjectTransformation<FileSystemInfo>(e => {
				FileInfo? fi = e as FileInfo;
				return new {
					e.CreationTimeUtc,
					e.Exists,
					e.FullName,
					IsReadOnly = fi?.IsReadOnly ?? false,
					e.LastWriteTimeUtc,
					Length = fi?.Length ?? int.MinValue,
				};
			});

			s.RegisterObjectTransformation<Type>(e => new {
				AssemblyFullName = e.Assembly.FullName,
				TypeFullName = e.FullName
			});
		});
	}

	/// <summary>
	/// Gets an (atomic) file target configured for writing out log files to disk.
	/// </summary>
	/// <remarks>
	/// <para><b>ARCHIVE NOTE:</b></para>
	/// Log file names should be "static" and placed in the <b>SAME</b> directory: dynamic replacement tokens in file names will cause issues with archiving. Additionally, log files
	/// are now created / archived where the highest (suffixed) file is the <i>NEWEST</i>.
	/// <para>Reference for additional options:</para>
	/// <para><see href="https://github.com/NLog/NLog/wiki/File-target"/></para>
	/// You may wish to wrap this in an <see cref="AsyncTargetWrapper"/> for higher performance:
	/// <para><see href="https://github.com/NLog/NLog/wiki/AsyncWrapper-target"/></para>
	/// <para>(Most of the default properties are good to go, just pay attention to <see cref="AsyncTargetWrapperOverflowAction"/>.)</para>
	/// </remarks>
	/// <param name="fileFullName">The absolute file name where NLog should write logs. This same file name / location serves also to be the location of archived logs, with a suffix where the highest numbered file is the newest/most current.</param>
	/// <param name="layout">The layout to be used for formatting log entries</param>
	/// <param name="archiveAboveSize">Size in bytes above which log files will be automatically archived.</param>
	/// <param name="maxArchiveDays">Maximum age of archive files in days to be kept.</param>
	/// <param name="maxArchiveFiles">Maximum number of archive files to retain. <c>-1</c> denotes that this feature is disabled.</param>
	/// <param name="name">Name of the target.</param>
	/// <returns><see cref="AtomicFileTarget"/></returns>
	public static AtomicFileTarget GetTargetAtomicFile(string fileFullName, Layout layout, long archiveAboveSize = 10000000, int maxArchiveDays = 188, int maxArchiveFiles = -1, string name = "") {
		return new AtomicFileTarget {
			ArchiveAboveSize = archiveAboveSize,
			ArchiveSuffixFormat = "_{0:000}",
			FileName = fileFullName.Replace('\\', '/'),
			Layout = layout,
			Name = name,
			MaxArchiveDays = maxArchiveDays,
			MaxArchiveFiles = maxArchiveFiles
		};
	}

	/// <summary>
	/// Gets a layout configured for json formatting
	/// </summary>
	/// <param name="recursionLimit">Maximum number of properties to use for recursion when writing out objects as Json</param>
	/// <param name="tzi"><see cref="TimeZoneInfo"/> used for log entries for converting the time stamp to local time when necessary. If <c>null</c> then <see cref="TimeZoneInfo.Local"/> will be used.</param>
	/// <returns><see cref="JsonLayout"/></returns>
	public static JsonLayout GetLayoutJson(int recursionLimit = 5, TimeZoneInfo? tzi = null) {
		var jsLayout = new JsonLayout {
			ExcludeEmptyProperties = true,
			MaxRecursionLimit = recursionLimit,
			RenderEmptyObject = false
		};

		jsLayout.Attributes.Add(new JsonAttribute("ID", Layout.FromString($"{DateTime.UtcNow.ToOADate()}_${{sequenceid}}")));
		jsLayout.Attributes.Add(new JsonAttribute("Level", Layout.FromString("${level:format=FirstCharacter}")));
		jsLayout.Attributes.Add(new JsonAttribute("Utc", Layout.FromString("${longdate}")));
		jsLayout.Attributes.Add(new JsonAttribute("Tzi", (tzi ?? TimeZoneInfo.Local).Id));
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
	/// Sets the value of <see cref="LogManager.Configuration"/> to a new configuration, configured for <see cref="LogLevel.Debug"/> to <see cref="LogLevel.Fatal"/>
	/// logging with the passed targets.
	/// </summary>
	/// <param name="configPostSet">Delegate that can be used to perform any other operation on the <see cref="LoggingConfiguration"/> after it has been set to <c>LogManager.Configuration</c>.</param>
	/// <param name="targets">Targets to add to the configuration.</param>
	/// <exception cref="ArgumentException"></exception>
	public static void SetLogManagerConfiguration(params Target[] targets) {
		if (targets is null || targets.Length < 1)
			throw new ArgumentException("Must have at least one target supplied");

		var config = new LoggingConfiguration();

		foreach (var target in targets)
			config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);

		LogManager.Configuration = config;
	}
}
