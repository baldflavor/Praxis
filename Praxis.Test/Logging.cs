namespace Praxis.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Logging;
using NLog;
using System.Reflection;

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

		foreach (var item in Enumerable.Range(1, 30_000).Select(e => new { Seq = e, Word = Praxis.Knowledge.Word.Random() })) {
			log.Info("Testing log of messages", item);
		}

		System.IO.File.WriteAllText(@"C:\Users\aaron.kropfreiter\OneDrive\Programming\SQL_Linq\Scratch.txt", $"[{string.Join(',', mTarget.Logs)}]");
	}
}
