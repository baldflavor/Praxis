namespace Praxis.Knowledge;

using System.Collections.ObjectModel;
using System.Reflection;

/// <summary>
/// Class used for working with and retrieving English language dictionary words. The list of words has been curated from it's original list
/// (e.g. removes words less than 3 characters, words like 'iii' and 'xiii')
/// </summary>
public static class Word {

	/// <summary>
	/// Gets a collection of all currently loaded words
	/// </summary>
	public static ReadOnlyCollection<string> Words { get; private set; }


	/// <summary>
	/// Performs static initialization of the <see cref="Word"/> class
	/// </summary>
	/// <exception cref="Exception"></exception>
	static Word() {
		using var stream =
				Assembly.GetExecutingAssembly()
				.GetManifestResourceStream("Praxis.Knowledge.EmbeddedResource.words_alpha_curated.txt") ??
				throw new Exception("Could not load the embedded word resource");

		using StreamReader sr = new(stream);
		string? line;
		List<string> words = [];

		while ((line = sr.ReadLine()) != null)
			words.Add(line!);

		Word.Words = new(words);
	}


	/// <summary>
	/// Gets a random word
	/// </summary>
	/// <returns>A string</returns>
	public static string Random() => Word.Words[System.Random.Shared.Next(0, Word.Words.Count)];


	/// <summary>
	/// Gets random words
	/// </summary>
	/// <param name="number">The number of words to return; clamped to 1 and the maximum number of words</param>
	/// <returns>A string</returns>
	public static string[] Random(int number) {
		number = Math.Clamp(number, 1, Word.Words.Count);

		string[] toReturn = new string[number];
		for (int i = 0; i < number; i++) {
			toReturn[i] = Word.Words[System.Random.Shared.Next(0, Word.Words.Count)];
		}

		return toReturn;
	}
}