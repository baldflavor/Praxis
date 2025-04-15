namespace Praxis.Cache;

/// <summary>
/// Class that represents a key and duration for something to be placed in cache
/// </summary>
/// <remarks>
/// Initializes a new instance of the KeyDuration class
/// </remarks>
/// <param name="key">The key to use</param>
/// <param name="duration">The duration value to use</param>
/// <param name="durationKind">The kind of duration (e.g.Seconds, Minutes, Hours) to use</param>
public sealed class KeyDuration(string key, int duration, DurationKind durationKind) {

	/// <summary>
	/// Gets or sets the duration of this object (i.e. time in cache)
	/// </summary>
	public int Duration { get; set; } = duration;

	/// <summary>
	/// Gets or sets how the duration value should be used in terms of time factoring
	/// </summary>
	public DurationKind DurationKind { get; set; } = durationKind;

	/// <summary>
	/// Gets or sets the key of this object
	/// </summary>
	public string Key { get; set; } = key;

	/// <summary>
	/// Implicitly converts this class to a string (key)
	/// </summary>
	/// <param name="kd">Target object</param>
	/// <returns>A string</returns>
	public static implicit operator string(KeyDuration kd) => kd.Key;

	/// <summary>
	/// Implicitly converts this class to an integer (duration)
	/// </summary>
	/// <param name="kd">Target object</param>
	/// <returns>A <c>TimeSpan</c> representing the key duration</returns>
	public static implicit operator TimeSpan(KeyDuration kd) {
		return
			kd.DurationKind switch {
				DurationKind.Hours => new TimeSpan(kd.Duration, 0, 0),
				DurationKind.Minutes => new TimeSpan(0, kd.Duration, 0),
				DurationKind.Seconds => new TimeSpan(0, 0, kd.Duration),
				_ => throw new Exception("Unrecognized DurationKind of " + kd.DurationKind + " encountered"),
			};
	}

	/// <summary>
	/// Returns the value of the <see cref="Key"/> property
	/// </summary>
	/// <returns>A string</returns>
	public override string ToString() => this.Key;
}