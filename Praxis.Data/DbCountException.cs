namespace Praxis.Data;

/// <summary>
/// Used when there was an unexpected number of some operation or result when working with a database
/// </summary>
public abstract class DbCountException : Exception {

	/// <summary>
	/// Constant to be used when some count was expected, but no specific value was known for what the count should have been at that time
	/// </summary>
	public const int NOTSPECIFIED = -1;

	/// <summary>
	/// Default message to use when storing this exception and no other message is specified
	/// </summary>
	private const string _DEFAULTMESSAGE = "The database reported a count of information that was unexpected.";


	/// <summary>
	/// Gets or sets the number of rows that the database reported / returned
	/// </summary>
	public int ActualCount { get; set; }

	/// <summary>
	/// Gets or sets an expected count resultant from database operation
	/// </summary>
	public int ExpectedCount { get; set; }

	/// <summary>
	/// Initializes a new instance of the DbCountException class.
	/// </summary>
	public DbCountException() {
	}

	/// <summary>
	/// Initializes a new instance of the DbCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	public DbCountException(string message) : base(message) {
	}

	/// <summary>
	/// Initializes a new instance of the DbCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="inner">Nested inner exception to be used</param>
	public DbCountException(string message, Exception inner) : base(message, inner) {
	}

	/// <summary>
	/// Initializes a new instance of the DbCountException class.
	/// </summary>
	/// <param name="expectedCount">The expected number of rows expected</param>
	/// <param name="reportedCount">The number of rows as reported</param>
	public DbCountException(int expectedCount, int reportedCount) : this(_DEFAULTMESSAGE) {
		this.ExpectedCount = expectedCount;
		this.ActualCount = reportedCount;
	}

	/// <summary>
	/// Initializes a new instance of the DbCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="expectedCount">The expected number of rows expected</param>
	/// <param name="reportedCount">The number of rows as reported</param>
	public DbCountException(string message, int expectedCount, int reportedCount) : this(message) {
		this.ExpectedCount = expectedCount;
		this.ActualCount = reportedCount;
	}

	/// <summary>
	/// Returns a string representation of this exception
	/// </summary>
	/// <returns>A string</returns>
	public override sealed string ToString() => $"{this.Message} (Expected: {this.ExpectedCount} Reported: {this.ActualCount})\r\n\r\n{base.ToString()}";
}