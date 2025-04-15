namespace Praxis.Data;

using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Class used for specifiying options when calling various sql extension methods
/// </summary>
public sealed class SqlExtensionOptions {

	/// <summary>
	/// Gets or sets a type of comment (or query) that is to be executed. The default is CommandType.Text
	/// </summary>
	public CommandType CommandType { get; set; }

	/// <summary>
	/// Gets or sets the timeout to use when executing the command. The default is 30 seconds.
	/// </summary>
	public int Timeout { get; set; }

	/// <summary>
	/// Gets or sets a transaction to be used when executing the query. Can be null.
	/// </summary>
	public SqlTransaction? Transaction { get; set; }

	/// <summary>
	/// Initializes a new instance of the SqlExtensionOptions class.
	/// </summary>
	public SqlExtensionOptions() {
		this.Timeout = 30;
	}

	/// <summary>
	/// Initializes a new instance of the SqlExtensionOptions class.
	/// </summary>
	/// <param name="commandType">Value to set for the CommandType property</param>
	/// <param name="transaction">Value to set for the Transaction property</param>
	/// <param name="timeout">Value to set for the timeout property</param>
	public SqlExtensionOptions(CommandType commandType, SqlTransaction? transaction = null, int timeout = 30) {
		this.CommandType = commandType;
		this.Transaction = transaction;
		this.Timeout = timeout;
	}
}