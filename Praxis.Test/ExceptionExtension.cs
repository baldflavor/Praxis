namespace Praxis.Test;

using System.Collections;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;
using Praxis.Logging;

[TestClass]
public class ExceptionExtension {

	[TestMethod]
	public void AddData_Various() {
		var mTarget = new NLog.Targets.MemoryTarget { Layout = NLogConfiguration.GetLayoutJson(), MaxLogsCount = 40_000 };
		NLogConfiguration.SetLogManagerConfiguration(mTarget);
		//NLog.LogManager.GetLogger(typeof(ExceptionExtension).FullName!).Initialized(new InitializeInfo(Assembly.GetExecutingAssembly()));
		var log = new LogBase(GetType());

		int num = Random.Shared.Next();
		try {
			_ThrowA(num);
		}
		catch (Exception ex) {
			var data = ex.Data;
			foreach (DictionaryEntry de in data) {
				var key = de.Key;
				var value = de.Value;
			}

			log.Error(ex);
		}

		foreach (string logEntry in mTarget.Logs) {
			var jNode = System.Text.Json.Nodes.JsonNode.Parse(logEntry);
			Assert.IsNotNull(jNode);
		}
	}



	private static void _ThrowA(int num) {
		try {
			_ThrowB(num, DateTimeOffset.UtcNow);
		}
		catch (Exception ex) {
			ex.AddData(num);
			throw;
		}
	}

	private static void _ThrowB(int num, DateTimeOffset tStamp) {
		try {
			_ThrowC(num, tStamp, Word.Random());
		}
		catch (Exception ex) {
			ex
				.AddData(num)
				.AddData(tStamp)
				.AddData(num)
				.AddData(new {
					Food = "Walnuts",
					IsCrunchy = true,
					DoubleMilliseconds = DateTime.Now.TimeOfDay.TotalMilliseconds
				},
				"OverriddenName");
			throw;
		}
	}

	private static void _ThrowC(int num, DateTimeOffset tStamp, string word) {
		try {
			throw new Exception("Thrown for exception add data testing");
		}
		catch (Exception ex) {
			ex
				.AddData(num)
				.AddData(tStamp)
				.AddData(word)
				.AddData(new {
					Word_Of_The_Day = Word.Random(),
					IsAnonymous = true,
					DoubleMilliseconds = DateTime.Now.TimeOfDay.TotalMilliseconds
				});
			throw;
		}
	}
}
