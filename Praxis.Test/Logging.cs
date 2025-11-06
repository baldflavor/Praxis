namespace Praxis.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Logging;
using NLog;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.Utilities;

[TestClass]
public class Logging {

	[TestMethod]
	public void Memory() {
		var mTarget = new NLog.Targets.MemoryTarget { Layout = NLogConfiguration.GetLayoutJson(), MaxLogsCount = 40_000 };

		var atWrapper = new NLog.Targets.Wrappers.AsyncTargetWrapper(mTarget) {
			OverflowAction = NLog.Targets.Wrappers.AsyncTargetWrapperOverflowAction.Grow
		};

		NLogConfiguration.SetLogManagerConfiguration(atWrapper);

		LogManager.GetLogger(typeof(Logging).FullName!).Initialized(new InitializeInfo(Assembly.GetExecutingAssembly()));

		LogBase log = new LogBase(GetType());

		foreach (var item in Enumerable.Range(1, 1_000).Select(e => new { Seq = e, Word = Knowledge.Word.Random() })) {
			log.Info("Testing log of messages", item);
		}

		string logOutput = $"[{string.Join(',', mTarget.Logs)}]";

		ConsoleOutput.Instance.WriteLine(logOutput, OutputLevel.Information);
	}
}
