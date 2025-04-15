namespace Praxis.Data;

using System;
using System.Data;
using System.Reflection;
using System.Text;

/// <summary>
/// Static class used for various DataTable/DataSet/etc type objects
/// </summary>
public static class Tools {

	/// <summary>
	/// Creates a string that represents a delimited data table
	/// <para>If the first column is named 'id' it will be output as 'recID' to prevent Excel from detecting a resultant CSV file as a SYLK file</para>
	/// <para>Guids are wrapped in {}</para>
	/// </summary>
	/// <param name="argTable">The datatable to parse out</param>
	/// <param name="delimiter">Delimiter to use for column separation</param>
	/// <param name="includeColumnNames">Include column names as the first row</param>
	/// <param name="quoteStrings">Quote string value columns</param>
	/// <returns>The string of the arg data table</returns>
	public static string DataTableToDelimited(DataTable argTable, string delimiter, bool includeColumnNames, bool quoteStrings) {
		StringBuilder sb = new();
		string quote = quoteStrings == true ? "\"" : string.Empty;
		int rowCount, columnCount, rowIndex, columnIndex;
		rowCount = argTable.Rows.Count;
		columnCount = argTable.Columns.Count;

		//Include column names in the generated file?
		if (includeColumnNames) {
			for (columnIndex = 0; columnIndex < columnCount; columnIndex++) {
				string colName;
				//This is here to prevent Excel from detecting a resultant CSV file as a SYLK file
				//It reads a file that starts with "ID" incorrectly/sloppily as such
				if (columnIndex == 0 && argTable.Columns[columnIndex].ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase)) {
					colName = "recID";
				}
				else {
					colName = argTable.Columns[columnIndex].ColumnName;
				}

				sb.Append(colName + (columnIndex < columnCount ? delimiter : string.Empty));
			}

			sb.Append(Environment.NewLine);
		}

		//Dump out each row and column
		for (rowIndex = 0; rowIndex < rowCount; rowIndex++) {
			DataRow dr = argTable.Rows[rowIndex];
			for (columnIndex = 0; columnIndex < columnCount; columnIndex++) {
				//Value of the column
				object drColVal = dr[columnIndex];
				string? dcVal = drColVal == DBNull.Value ? null : drColVal.ToString();

				//DataType of column
				Type colType = argTable.Columns[columnIndex].DataType;

				if (colType == typeof(string))
					sb.Append(quote + (dcVal ?? "") + quote + (columnIndex < columnCount - 1 ? delimiter : ""));
				else if (colType == typeof(Guid))
					sb.Append(quote + (dcVal != null ? "{" + dcVal + "}" : "") + quote + (columnIndex < columnCount - 1 ? delimiter : ""));
				else
					sb.Append((dcVal ?? "") + (columnIndex < columnCount ? delimiter : ""));
			}

			if (rowIndex < rowCount - 1)
				sb.Append(Environment.NewLine);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Converts an IEnumerable of [T] items to a datatable (searches for public properties)
	/// </summary>
	/// <typeparam name="T">The type of items inferred from the list to be converted</typeparam>
	/// <param name="items">A list of items to convert; can be empty but not null</param>
	/// <returns>A datatable</returns>
	public static DataTable IEnumerableToDataTable<T>(IEnumerable<T> items) {
		ArgumentNullException.ThrowIfNull(items);

		Type t = typeof(T);

		DataTable dt = new(t.Name);

		PropertyInfo[] pis = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo pi in pis) {
			//Nullable.GetUnderlying type returns 'null' if the passed type isn't a nullable type
			//http://msdn.microsoft.com/en-us/library/system.nullable.getunderlyingtype.aspx
			dt.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
		}

		int pisLen = pis.Length;
		foreach (T item in items) {
			object[] values = new object[pisLen];
			for (int i = 0; i < pisLen; i++)
				values[i] = pis[i].GetValue(item, null) ?? DBNull.Value;
			dt.Rows.Add(values);
		}

		return dt;
	}
}