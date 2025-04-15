namespace Praxis.Data;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

/// <summary>
/// Static class used for projecting data rows, tables, readers, and sets into concrete objects
/// <para>Uses a dictionary to hold (and provide) property information by Type.</para>
/// </summary>
public static class DataRowProjector {

	/// <summary>
	/// Nested dictionary that is keyed by assembly qualified type name, and then contains a dictionary of property info objects keyed by their Name
	/// </summary>
	private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _typePropDict = new();

	/// <summary>
	/// Gets a dictionary of property info given the specified type, using the name of each property as the case insensitive key
	/// </summary>
	/// <typeparam name="T">Type of T to use as a arg</typeparam>
	/// <returns>A dictionary keyed by property name (case insensitive)</returns>
	public static Dictionary<string, PropertyInfo> GetPropertyInfos<T>() where T : class {
		Type arg = typeof(T);
		// Only bind base data types (by the assembly of "int" or where they are enums. (i.e. Don't bind any other rich types or classes)
		if (!_typePropDict.ContainsKey(arg))
			_typePropDict.TryAdd(arg, arg.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => pi.PropertyType.Assembly == Assembly.GetAssembly(typeof(int)) || pi.PropertyType.IsEnum).ToDictionary(pi => pi.Name, StringComparer.OrdinalIgnoreCase));
		return _typePropDict[arg];
	}

	/// <summary>
	/// Projects data row information into the properties of a class T. Only public instance properties are arged.
	/// <para>If a column (name) does not match a property (name) then it is ignored</para>
	/// <para>Beware of type mismatches however between a column and property type</para>
	/// <para>Will attempt to correctly set Enum.Parsed values for enumeration properties</para>
	/// </summary>
	/// <typeparam name="T">The requested type to be instantiated/filled</typeparam>
	/// <param name="dr">A data row that contains the values to set on the model; ONLY NON-NULL DATA is set on the object</param>
	/// <param name="fillTo">If not null, will be used as the arg of property set/return, rather than a newly instantiated object</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment and ignores other columns</param>
	/// <param name="propDict">Optional dictionary to pass into the method rather than having it looked up by type.</param>
	/// <returns>A class of T with properties set against the data row</returns>
	public static T Project<T>(this DataRow dr, T? fillTo = null, string? prefix = null, Dictionary<string, PropertyInfo>? propDict = null) where T : class {
		if (dr == null)
			throw new ArgumentException("DataRow was null", nameof(dr));

		if (dr.Table.Columns.Count < 1)
			throw new ArgumentException("Passed data row's corresponding table had no columns", "dr.Table.Columns");

		try {
			T toReturn = fillTo ?? Activator.CreateInstance<T>();

			propDict ??= GetPropertyInfos<T>();

			Func<string, PropertyInfo>? getPropertyInfoByColumn = null;

			IEnumerable<DataColumn> columns;
			if (string.IsNullOrWhiteSpace(prefix)) {
				columns = dr.Table.Columns.Cast<DataColumn>();

				getPropertyInfoByColumn = (cName) => {
					propDict.TryGetValue(cName, out PropertyInfo? pi);
					return pi!;
				};
			}
			else {
				columns = dr.Table.Columns.Cast<DataColumn>().Where(dc => dc.ColumnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

				getPropertyInfoByColumn = (cName) => {
					propDict.TryGetValue(cName[prefix.Length..], out PropertyInfo? pi);
					return pi!;
				};
			}

			if (!columns.Any())
				throw new Exception("No columns were present to use for conversion of row to model");

			foreach (DataColumn dc in columns) {
				PropertyInfo propInfo = getPropertyInfoByColumn(dc.ColumnName);

				if (propInfo == null)
					continue;

				object drColVal = dr[dc];

				if (propInfo.PropertyType.IsEnum)
					propInfo.SetValue(toReturn, Enum.Parse(propInfo.PropertyType, drColVal.ToString()!), null);
				else
					propInfo.SetValue(toReturn, drColVal == DBNull.Value ? null : drColVal, null);
			}

			return toReturn;
		}
		catch (Exception ex) {
			if (dr != null) {
				foreach (DataColumn dc in dr.Table.Columns)
					ex.Data["DC_" + dc.ColumnName] = dr[dc] == Convert.DBNull ? "DBNull" : dr[dc].ToString();
			}

			throw;
		}
	}

	/// <summary>
	/// Using all the rows in the given DataTable, performs a DataRow.Project operation on each row. See that method for more information.
	/// </summary>
	/// <typeparam name="T">The type of object to be projected into for each row</typeparam>
	/// <param name="dt">DataTable to use for projection</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment and ignores other columns</param>
	/// <returns>A list of T objects</returns>
	public static List<T> Project<T>(this DataTable dt, string? prefix = null) where T : class {
		ArgumentNullException.ThrowIfNull(dt);

		if (dt.Columns.Count < 1)
			throw new ArgumentException("Passed data row's corresponding table had no columns", "dt.Columns");

		try {
			int rowCount = dt.Rows.Count;
			var toReturn = new List<T>(rowCount);
			Dictionary<string, PropertyInfo> propDict = GetPropertyInfos<T>();

			for (int i = 0; i < rowCount; i++)
				toReturn.Add(dt.Rows[i].Project<T>(propDict: propDict, prefix: prefix));

			return toReturn;
		}
		catch (Exception ex) {
			if (dt != null) {
				foreach (DataColumn dc in dt.Columns)
					ex.Data["DC_" + dc.ColumnName] = dc.DataType.ToString();

				ex.Data["Row Count"] = dt.Rows.Count;
			}

			throw;
		}
	}

	/// <summary>
	/// Projects a data set and it's tables / data row information into a Tuple of respective Lists by type
	/// <para>Uses DataTable.Project for operation. See that method for more information</para>
	/// </summary>
	/// <typeparam name="T1">The requested type to be instantiated/filled from the first table</typeparam>
	/// <typeparam name="T2">The requested type to be instantiated/filled from the second table</typeparam>
	/// <param name="ds">A data set that contains data tables that contain rows use for projecting models; only non-null data is set on the object</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment</param>
	/// <returns>A Tuple of lists of the passed types with properties set against the data rows.</returns>
	public static Tuple<List<T1>, List<T2>> Project<T1, T2>(this DataSet ds, string? prefix = null)
		where T1 : class
		where T2 : class {
		ArgumentNullException.ThrowIfNull(ds);

		Type[] typeArgs = MethodBase.GetCurrentMethod()?.GetGenericArguments() ?? throw new Exception("MethodBase.GetCurrentMethod() returned a null reference");

		int tableCount = ds.Tables.Count;
		if (tableCount != typeArgs.Length)
			throw new TableCountException(typeArgs.Length, tableCount);

		return new Tuple<List<T1>, List<T2>>(
				ds.Tables[0].Project<T1>(prefix),
				ds.Tables[1].Project<T2>(prefix));
	}

	/// <summary>
	/// Projects a data set and it's tables / data row information into a Tuple of respective Lists by type
	/// <para>Uses DataTable.Project for operation. See that method for more information</para>
	/// </summary>
	/// <typeparam name="T1">The requested type to be instantiated/filled from the first table</typeparam>
	/// <typeparam name="T2">The requested type to be instantiated/filled from the second table</typeparam>
	/// <typeparam name="T3">The requested type to be instantiated/filled from the third table</typeparam>
	/// <param name="ds">A data set that contains data tables that contain rows use for projecting models; only non-null data is set on the object</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment</param>
	/// <returns>A Tuple of lists of the passed types with properties set against the data rows.</returns>
	public static Tuple<List<T1>, List<T2>, List<T3>> Project<T1, T2, T3>(this DataSet ds, string? prefix = null)
		where T1 : class
		where T2 : class
		where T3 : class {
		ArgumentNullException.ThrowIfNull(ds);

		Type[] typeArgs = MethodBase.GetCurrentMethod()?.GetGenericArguments() ?? throw new Exception("MethodBase.GetCurrentMethod() returned a null reference");

		int tableCount = ds.Tables.Count;
		if (tableCount != typeArgs.Length)
			throw new TableCountException(typeArgs.Length, tableCount);

		return new Tuple<List<T1>, List<T2>, List<T3>>(
				ds.Tables[0].Project<T1>(prefix),
				ds.Tables[1].Project<T2>(prefix),
				ds.Tables[2].Project<T3>(prefix));
	}

	/// <summary>
	/// Projects a data set and it's tables / data row information into a Tuple of respective Lists by type
	/// <para>Uses DataTable.Project for operation. See that method for more information</para>
	/// </summary>
	/// <typeparam name="T1">The requested type to be instantiated/filled from the first table</typeparam>
	/// <typeparam name="T2">The requested type to be instantiated/filled from the second table</typeparam>
	/// <typeparam name="T3">The requested type to be instantiated/filled from the third table</typeparam>
	/// <typeparam name="T4">The requested type to be instantiated/filled from the fourth table</typeparam>
	/// <param name="ds">A data set that contains data tables that contain rows use for projecting models; only non-null data is set on the object</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment</param>
	/// <returns>A Tuple of lists of the passed types with properties set against the data rows.</returns>
	public static Tuple<List<T1>, List<T2>, List<T3>, List<T4>> Project<T1, T2, T3, T4>(this DataSet ds, string? prefix = null)
		where T1 : class
		where T2 : class
		where T3 : class
		where T4 : class {
		ArgumentNullException.ThrowIfNull(ds);

		Type[] typeArgs = MethodBase.GetCurrentMethod()?.GetGenericArguments() ?? throw new Exception("MethodBase.GetCurrentMethod() returned a null reference");

		int tableCount = ds.Tables.Count;
		if (tableCount != typeArgs.Length)
			throw new TableCountException(typeArgs.Length, tableCount);

		return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(
				ds.Tables[0].Project<T1>(prefix),
				ds.Tables[1].Project<T2>(prefix),
				ds.Tables[2].Project<T3>(prefix),
				ds.Tables[3].Project<T4>(prefix));
	}

	/// <summary>
	/// Projects a data set and it's tables / data row information into a Tuple of respective Lists by type
	/// <para>Uses DataTable.Project for operation. See that method for more information</para>
	/// </summary>
	/// <typeparam name="T1">The requested type to be instantiated/filled from the first table</typeparam>
	/// <typeparam name="T2">The requested type to be instantiated/filled from the second table</typeparam>
	/// <typeparam name="T3">The requested type to be instantiated/filled from the third table</typeparam>
	/// <typeparam name="T4">The requested type to be instantiated/filled from the fourth table</typeparam>
	/// <typeparam name="T5">The requested type to be instantiated/filled from the fifth table</typeparam>
	/// <param name="ds">A data set that contains data tables that contain rows use for projecting models; only non-null data is set on the object</param>
	/// <param name="prefix">Specifies a prefix for data columns to EXCLUSIVELY use for data assignment</param>
	/// <returns>A Tuple of lists of the passed types with properties set against the data rows.</returns>
	public static Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Project<T1, T2, T3, T4, T5>(this DataSet ds, string? prefix = null)
		where T1 : class
		where T2 : class
		where T3 : class
		where T4 : class
		where T5 : class {
		ArgumentNullException.ThrowIfNull(ds);

		Type[] typeArgs = MethodBase.GetCurrentMethod()?.GetGenericArguments() ?? throw new Exception("MethodBase.GetCurrentMethod() returned a null reference");

		int tableCount = ds.Tables.Count;
		if (tableCount != typeArgs.Length)
			throw new TableCountException(typeArgs.Length, tableCount);

		return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>(
				ds.Tables[0].Project<T1>(prefix),
				ds.Tables[1].Project<T2>(prefix),
				ds.Tables[2].Project<T3>(prefix),
				ds.Tables[3].Project<T4>(prefix),
				ds.Tables[4].Project<T5>(prefix));
	}
}