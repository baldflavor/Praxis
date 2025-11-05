namespace Praxis.Test;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;

[TestClass]
public class ObjectExtension {

	[TestMethod]
	public void PushTo() {

		var movieStub = new MovieStub {
			AwardsWon = 20,
			GeNrE = "Action",
			// LengthMinutes -> nullable value type should set int default on movie since it is not a nullable value type
			Rating = "PG-13",
			TagLine = "DO NOT SET ME ON THE OTHER OBJECT",
			TrackingID = Guid.NewGuid()
		};

		var movie = new Movie {
			AwardsWon = 10,
			ColorSpace = ColorSpace.Color,
			Genre = "UNKNOWN",
			LengthMinutes = 94,
			Promotion = new Promotion { Code = "SBC12", DiscountPercent = 0.12 },
			Rating = "UNKNOWN",
			TagLine = "A cop on the edge starts blasting bad guys!",
			Title = "Super Blasto Cop"
		};

		movieStub.PushTo(movie, true, true, nameof(MovieStub.TagLine));
		Assert.AreEqual(movie.Genre, movieStub.GeNrE);
		Assert.AreEqual(10, movie.AwardsWon);


		movieStub = new MovieStub {
			AwardsWon = 20,
			GeNrE = "Action",
			IsInTheaters = true,
			// LengthMinutes -> nullable value type should set int default on movie since it is not a nullable value type
			Rating = "PG-13",
			TagLine = "DO NOT SET ME ON THE OTHER OBJECT",
			TrackingID = Guid.NewGuid()
		};

		movie = new Movie {
			AwardsWon = 10,
			ColorSpace = ColorSpace.Color,
			Genre = "UNKNOWN",
			IsInTheaters = false,
			LengthMinutes = 94,
			Promotion = new Promotion { Code = "SBC12", DiscountPercent = 0.12 },
			Rating = "UNKNOWN",
			TagLine = "A cop on the edge starts blasting bad guys!",
			Title = "Super Blasto Cop"
		};

		movieStub.PushTo(movie, true, false);
		Assert.IsTrue(movie.IsInTheaters);
		Assert.AreEqual("UNKNOWN", movie.Rating);
	}





	private class Movie {
		public bool IsInTheaters;

		public ColorSpace? ColorSpace { get; set; }

		public required string Genre { get; set; }

		public int LengthMinutes { get; set; }

		public required string Rating { get; set; }

		public required string TagLine { get; set; }

		public required string Title { get; set; }

		public int AwardsWon { get; set; }

		public required Promotion Promotion { get; set; }
	}

	private class MovieStub {
		public bool IsInTheaters;

		public short AwardsWon { get; set; }

		public ColorSpace? ColorSpace { get; set; }

		public required string GeNrE { get; set; }

		public int? LengthMinutes { get; set; }

		public string? Rating { get; set; }

		public required string TagLine { get; set; }

		public Guid TrackingID { get; set; }

		public Promotion? Promotion { get; set; }
	}

	private class Promotion {
		public required string Code { get; set; }

		public double DiscountPercent { get; set; }
	}

	private enum ColorSpace {
		Unknown,
		BlackAndWhite,
		Color,
		HDR
	}
}
