namespace Praxis.Test;

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;
using Praxis.LiteDb;

[TestClass]
public class LiteDb {
	// Static reference to a mapper used during connection to a LiteDatabase
	private static BsonMapper _bMapper =
		// Comment / uncomment to see the effect this has on dates
		FactoryOption.RegisterCustomDateTimeDateTimeOffsetMapping(
			new BsonMapper { EnumAsInteger = true, TrimWhitespace = false }
		);

	[TestMethod]
	public void SerializeDeserializeDates() {
		string dbFileName = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
		Debug.WriteLine($"DB FILE NAME: {dbFileName}");

		try {
			var lr = _GetLiteRepository(dbFileName, false);

			DateTime bDate = new DateTime(DateOnly.FromDateTime(DateTime.Now), TimeOnly.MinValue, DateTimeKind.Unspecified);

			var entities =
				new List<LDBAttack>(Enumerable.Range(0, 4).Select(e => new LDBAttack {
					AttackKind = Random.Shared.GetItems(Enum.GetValues<AttackKind>(), 1).Single(),
					Batch = "A",
					CreatedOn = bDate = bDate.AddDays(1).AddHours(1.5),
					DamageValue = (decimal)Random.Shared.NextDouble() * 100,
					IsActive = Random.Shared.Next(2) == 1,
					LDBAttackID = ObjectId.NewObjectId(),
				}))
				.ToArray();

			foreach (var entity in entities)
				lr.Insert(entity);

			var utcFalseResults = lr.Database.GetCollection<LDBAttack>().FindAll().ToArray();

			lr.Dispose();
			lr = _GetLiteRepository(dbFileName, true);

			foreach (var entity in entities) {
				entity.Batch = "B";
				entity.LDBAttackID = new ObjectId();
				lr.Insert(entity);
			}

			lr.Database.Checkpoint();

			DateTime filter =
				DateTime.MinValue
				//entities[1].CreatedOn
				;

			var utcTrueResults = lr.Query<LDBAttack>().Where(e => e.Batch == "B" && e.CreatedOn > filter).OrderBy(r => r.Index).ToArray();

			lr.Dispose();

			// Note the difference in the dates and times and how the "UtcDate" pragma of the connection causes weird issues
			var allResults = utcFalseResults.Concat(utcTrueResults).OrderBy(r => r.Index).ThenBy(r => r.Batch).Select(e => new { Idx = e.Index, Bt = e.Batch, CO = e.CreatedOn, K = e.CreatedOn.Kind }).ToArray();
		}
		finally {
			if (File.Exists(dbFileName))
				File.Delete(dbFileName);
		}
	}

	private static LiteRepository _GetLiteRepository(string fileFullName, bool utc) {
		var conString = new ConnectionString {
			Connection = ConnectionType.Shared,
			Filename = fileFullName
		};

		if (!File.Exists(fileFullName))
			conString.Collation = FactoryOption.InvariantIgnoreCaseCollation;

		LiteRepository lr = new(conString, _bMapper);

		lr.Database.UtcDate = utc;
		return lr;
	}


	// Used for testing functions of LiteDb
	public record class LDBAttack {
		/// <summary>
		/// Indexer tracking object created instances
		/// </summary>
		private static int _index;

		/// <summary>
		/// Identifier for this instance
		/// </summary>
		public required LiteDB.ObjectId LDBAttackID { get; set; }

		/// <summary>
		/// Gets or sets an index number for sequencing
		/// </summary>
		public int Index { get; set; } = _index++;

		/// <summary>
		/// Gets or sets the batch on when inserted
		/// </summary>
		public required string Batch { get; set; }

		/// <summary>
		/// Gets or sets a kind of attack
		/// </summary>
		public required AttackKind AttackKind { get; init; }

		public string AttackKindString => this.AttackKind.ToString();

		/// <summary>
		/// Gets or sets when this was created
		/// </summary>
		public required DateTime CreatedOn { get; init; }

		/// <summary>
		/// Gets or sets if the widget is active
		/// </summary>
		public required bool IsActive { get; init; }

		/// <summary>
		/// Gets or sets how much damage the attack does
		/// </summary>
		public required decimal DamageValue { get; init; }
	}

	/// <summary>
	/// A kind of attack
	/// </summary>
	public enum AttackKind {
		Battering,

		Headbutt,

		Punch,

		Kick,

		Suplex,

		Piledriver,

		ElbowStrike,

		Bite
	}
}
