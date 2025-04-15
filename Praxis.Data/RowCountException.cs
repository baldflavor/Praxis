namespace Praxis.Data;

/// <summary>
/// Used when there was an unexpected number of rows returned as a result of a query
/// </summary>
public class RowCountException : DbCountException {

	/// <summary>
	/// Initializes a new instance of the RowCountException class.
	/// </summary>
	public RowCountException() {
	}

	/// <summary>
	/// Initializes a new instance of the RowCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	public RowCountException(string message) : base(message) {
	}

	/// <summary>
	/// Initializes a new instance of the RowCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="inner">Nested inner exception to be used</param>
	public RowCountException(string message, Exception inner) : base(message, inner) {
	}

	/// <summary>
	/// Initializes a new instance of the RowCountException class.
	/// </summary>
	/// <param name="expectedCount">The expected number of rows expected</param>
	/// <param name="actualCount">The number of rows as reported</param>
	public RowCountException(int expectedCount, int actualCount) : base(expectedCount, actualCount) {
	}

	/// <summary>
	/// Initializes a new instance of the RowCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="expectedCount">The expected number of rows expected</param>
	/// <param name="actualCount">The number of rows as reported</param>
	public RowCountException(string message, int expectedCount, int actualCount) : base(message, expectedCount, actualCount) {
	}
}