namespace Praxis.Data;

using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Extensions that can be used on SqlConnections
/// </summary>
public static class SqlConnectionExtensions {

	/// <summary>
	/// Executes a scalar database query
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command text to use</param>
	/// <param name="parms">Parameters to use</param>
	/// <typeparam name="T">The resulting expected value/type returned by the query</typeparam>
	/// <returns>The T boxed value of the first row, first column of execution; if the result is DbNull then the default of T will be returned</returns>
	/// <exception cref="Exception">May be thrown if a failure occurs during execution/operation</exception>
	public static async Task<T?> ExecuteScalar<T>(this SqlConnection connection, string commandText, params SqlParameter[] parms) => await ExecuteScalar<T>(connection, commandText, new SqlExtensionOptions(), parms).ConfigureAwait(false);


	/// <summary>
	/// Executes a scalar database query
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command text to use</param>
	/// <param name="options">Options to use when executing</param>
	/// <param name="parms">Parameters to use</param>
	/// <typeparam name="T">The resulting expected value/type returned by the query</typeparam>
	/// <returns>The T boxed value of the first row, first column of execution; if the result is DbNull then the default of T will be returned</returns>
	/// <exception cref="Exception">May be thrown if a failure occurs during execution/operation</exception>
	public static async Task<T?> ExecuteScalar<T>(this SqlConnection connection, string commandText, SqlExtensionOptions options, params SqlParameter[] parms) {
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using SqlCommand command = new() {
			CommandType = options.CommandType,
			Connection = connection,
			CommandText = commandText,
			CommandTimeout = options.Timeout
		};

		if (options.Transaction != null)
			command.Transaction = options.Transaction;

		if (parms != null && parms.Length > 0)
			command.Parameters.AddRange(parms);

		//Execute scalar statement and return boxed value
		try {
			object? result = await command.ExecuteScalarAsync().ConfigureAwait(false);
			if (result == DBNull.Value)
				return default;
			else
				return (T?)result;
		}
		finally {
			command.Parameters.Clear();
		}
	}

	/// <summary>
	/// Returns a data set retrieved using the passed parameters
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// <para>YOU ARE RESPONSIBLE FOR DISPOSING OF THE DATASET WHEN NO LONGER NEEDED</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command string to use for execution</param>
	/// <param name="parms">SqlParameters to use when executing query</param>
	/// <returns>A dataset</returns>
	/// <exception cref="Exception">If dataset is null or contains no tables</exception>
	public static DataSet GetDataSet(this SqlConnection connection, string commandText, params SqlParameter[] parms) => GetDataSet(connection, commandText, new SqlExtensionOptions(), parms);

	/// <summary>
	/// Returns a data set retrieved using the passed parameters
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// <para>YOU ARE RESPONSIBLE FOR DISPOSING OF THE DATASET WHEN NO LONGER NEEDED</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command string to use for execution</param>
	/// <param name="options">Options to use when executing</param>
	/// <param name="parms">SqlParameters to use when executing query</param>
	/// <returns>A dataset</returns>
	/// <exception cref="Exception">If dataset is null or contains no tables</exception>
	public static DataSet GetDataSet(this SqlConnection connection, string commandText, SqlExtensionOptions options, params SqlParameter[] parms) {
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using SqlCommand command = new() {
			CommandType = options.CommandType,
			Connection = connection,
			CommandText = commandText,
			CommandTimeout = options.Timeout
		};

		if (options.Transaction != null)
			command.Transaction = options.Transaction;

		if (parms.Length > 0)
			command.Parameters.AddRange(parms);

		try {
			//Create, fill, cleanup adapter and command
			using SqlDataAdapter adapter = new(command);
			DataSet ds = new();
			adapter.Fill(ds);

			if (ds.Tables.Count < 1) {
				ds.Dispose();
				TableCountException tce = new(DbCountException.NOTSPECIFIED, ds.Tables.Count);
				tce.Data["Command"] = commandText;
				foreach (SqlParameter parm in parms) {
					tce.Data["Parameter " + parm.ParameterName] = parm.Value;
				}

				throw tce;
			}

			return ds;
		}
		finally {
			command.Parameters.Clear();
		}
	}

	/// <summary>
	/// Returns a datatable retrieved using the passed parameters
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// <para>YOU ARE RESPONSIBLE FOR DISPOSING OF THE DATATABLE WHEN NO LONGER NEEDED</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command string to use for execution</param>
	/// <param name="parms">SqlParameters to use when executing query</param>
	/// <returns>A datatable of rows</returns>
	public static DataTable GetDataTable(this SqlConnection connection, string commandText, params SqlParameter[] parms) => GetDataTable(connection, commandText, new SqlExtensionOptions(), parms);

	/// <summary>
	/// Returns a datatable retrieved using the passed parameters
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// <para>YOU ARE RESPONSIBLE FOR DISPOSING OF THE DATATABLE WHEN NO LONGER NEEDED</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command string to use for execution</param>
	/// <param name="options">Options to use when executing</param>
	/// <param name="parms">SqlParameters to use when executing query</param>
	/// <returns>A datatable of rows</returns>
	public static DataTable GetDataTable(this SqlConnection connection, string commandText, SqlExtensionOptions options, params SqlParameter[] parms) {
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using SqlCommand command = new() {
			CommandType = options.CommandType,
			Connection = connection,
			CommandText = commandText,
			CommandTimeout = options.Timeout
		};

		if (options.Transaction != null)
			command.Transaction = options.Transaction;

		if (parms.Length > 0)
			command.Parameters.AddRange(parms);

		try {
			//Create, fill, cleanup adapter and command
			using SqlDataAdapter adapter = new(command);
			DataTable dt = new();
			adapter.Fill(dt);

			if (dt == null) {
				TableCountException tce = new(1, 0);
				tce.Data["Command"] = commandText;
				foreach (SqlParameter parm in parms) {
					tce.Data["Parameter " + parm.ParameterName] = parm.Value;
				}

				throw tce;
			}

			return dt;
		}
		finally {
			command.Parameters.Clear();
		}
	}

	/// <summary>
	/// Returns the current UTC date and time as reported by the server
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="transaction">Transaction to use/enlist if not null</param>
	/// <returns>A DateTimeOffset value</returns>
	/// <exception cref="Exception">Thrown on operational error, or if the data returned from the server is not an actual date and time</exception>
	public static async Task<DateTimeOffset> GetServerDate(this SqlConnection connection, SqlTransaction? transaction = null) {
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using SqlCommand command = new("SELECT GETUTCDATE()", connection);

		try {
			if (transaction != null)
				command.Transaction = transaction;
			return (DateTimeOffset)((await command.ExecuteScalarAsync().ConfigureAwait(false)) ?? throw new Exception("Data not present"));
		}
		catch (Exception ex) {
			Exception ex2 = new("A result was returned from the database but it was not of the DateTime type and therefore could not be obtained.", ex);
			throw ex2;
		}
	}

	/// <summary>
	/// Runs an insert, update, or delete statement against a database connection
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command text to use</param>
	/// <param name="parms">Parameters to use</param>
	/// <returns>The return value of the operation.  For many stored procedures this is -1 (if 'no count' is on)</returns>
	/// <exception cref="Exception">May be thrown if a failure occurs during execution/operation</exception>
	public static async Task<int> InsertUpdateDelete(this SqlConnection connection, string commandText, params SqlParameter[] parms) => await InsertUpdateDelete(connection, commandText, new SqlExtensionOptions(), parms).ConfigureAwait(false);

	/// <summary>
	/// Runs an insert, update, or delete statement against a database connection
	/// <para>YOU ARE STILL RESPONSIBLE FOR CLOSING/DISPOSING THE PASSED CONNECTION</para>
	/// </summary>
	/// <param name="connection">SqlConnection to use; will open if State == Closed</param>
	/// <param name="commandText">The command text to use</param>
	/// <param name="options">Options to use when executing</param>
	/// <param name="parms">Parameters to use</param>
	/// <returns>The return value of the operation.  For many stored procedures this is -1 (if 'no count' is on)</returns>
	/// <exception cref="Exception">May be thrown if a failure occurs during execution/operation</exception>
	public static async Task<int> InsertUpdateDelete(this SqlConnection connection, string commandText, SqlExtensionOptions options, params SqlParameter[] parms) {
		if (connection.State == ConnectionState.Closed)
			connection.Open();

		using SqlCommand command = new() {
			CommandType = options.CommandType,
			Connection = connection,
			CommandText = commandText,
			CommandTimeout = options.Timeout
		};

		if (options.Transaction != null)
			command.Transaction = options.Transaction;

		if (parms != null && parms.Length > 0)
			command.Parameters.AddRange(parms);

		//Execute -- Note that in stored procedures (or in general) having NOCOUNT ON will return -1
		//to this end, it might be beneficial to have the stored procedure explicitly return a '1' on
		//a successful operation if you are depending on the return value for validation
		try {
			return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
		finally {
			command.Parameters.Clear();
		}
	}
}