namespace Praxis.Data;

/// <summary>
/// Used when there was an unexpected number of Tables returned as a result of a query
/// </summary>
[Serializable]
public class TableCountException : DbCountException {

	/// <summary>
	/// Initializes a new instance of the TableCountException class.
	/// </summary>
	public TableCountException() {
	}

	/// <summary>
	/// Initializes a new instance of the TableCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	public TableCountException(string message) : base(message) {
	}

	/// <summary>
	/// Initializes a new instance of the TableCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="inner">Nested inner exception to be used</param>
	public TableCountException(string message, Exception inner) : base(message, inner) {
	}

	/// <summary>
	/// Initializes a new instance of the TableCountException class.
	/// </summary>
	/// <param name="expectedCount">The expected number of Tables expected</param>
	/// <param name="actualCount">The number of Tables as reported</param>
	public TableCountException(int expectedCount, int actualCount) : base(expectedCount, actualCount) {
	}

	/// <summary>
	/// Initializes a new instance of the TableCountException class.
	/// </summary>
	/// <param name="message">Provides a message explaining the exception circumstances</param>
	/// <param name="expectedCount">The expected number of Tables expected</param>
	/// <param name="actualCount">The number of Tables as reported</param>
	public TableCountException(string message, int expectedCount, int actualCount) : base(message, expectedCount, actualCount) {
	}
}