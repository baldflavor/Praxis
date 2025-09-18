namespace Praxis.Test;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;

[TestClass]
public class ObjectExtension {

	[TestMethod]
	public void PushPropertiesTo() {

		var film = new Film {
			Genre = "Horror",
			Title = "Slasher At Slashy Lake"
		};

		var movie = new Movie {
			Genre = "Action",
			Title = "Super Blasto Cop",
			LengthMinutes = 94,
			Rating = "R"
		};

		film.PushTo(movie);



	}





	private class Movie {
		public required string Genre { get; set; }

		public int LengthMinutes { get; set; }

		public required string Rating { get; set; }

		public required string Title { get; set; }
	}

	private class Film {
		public required string Genre { get; set; }

		public int? LengthMinutes { get; set; }

		public string? Rating { get; set; }

		public required string Title { get; set; }
	}
}
