namespace Praxis.Interface;

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;
using Praxis.Attribute;

/// <summary>
/// Interface to support a class that supports html formatting
/// </summary>
public interface ITabularDisplay {

	/// <summary>
	/// Constant field that indicates a starting index "sort" value for those properties that do not specify order via <see cref="TabularDisplayAttribute.GetOrder"/> or where that value is null
	/// </summary>
	private const int _INITIALIDX = 2099999999;

	/// <summary>
	/// Builds an html table off of the passed <see cref="ITabularDisplay"/> implementing classes
	/// </summary>
	/// <param name="data">Data to use for generation of an html table</param>
	/// <returns>A string that contains valid html - table data</returns>
	public static string BuildHtml(IEnumerable<ITabularDisplay> data) {
		if (data?.Any() != true)
			throw new ArgumentException("Appreciable data is required for output generation", nameof(data));

		StringBuilder sb = new();

		sb.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");
		sb.Append("<tr>");

		foreach (string header in data.First().Headers()) {
			sb.AppendFormat("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>{0}</th>", header);
		}

		sb.Append("</tr>");

		foreach (ITabularDisplay d in data) {
			sb.Append("<tr>");
			foreach (string td in d.Data()) {
				sb.Append($"<td style='text-align:left;border: 1px solid #ccc'>{td ?? "&nbsp;"}</td>");
			}

			sb.Append("</tr>");
		}

		sb.Append("</table>");
		return sb.ToString();
	}

	/// <summary>
	/// Gets header names represented by a class. The default implementation will use reflection to grab all public instance names that can be
	/// read, and will use the <see cref="DisplayAttribute.Name"/> property, or if null / not present, the <see cref="MemberInfo.Name"/>
	/// <para>Additionally, using the <see cref="DisplayAttribute.Order"/> attribute will order data as specified,
	/// those without will begin ordering at <see cref="_INITIALIDX"/>. Sorting is then performed by name to account for collisions or group-sorting</para>
	/// </summary>
	/// <returns>An array of strings</returns>
	public static string[] Data(object arg, string dateFormat = "MM/dd/yyyy hh:mm:ss", string floatingNumberFormat = "0.00", string guidFormat = "N") {
		int idx = _INITIALIDX;
		return
			(from p in arg.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
			 let dattr = p.GetCustomAttribute<TabularDisplayAttribute>()
			 let name = dattr?.Label ?? p.Name
			 let val = p.GetValue(arg)
			 let propType = _GetNullableSafeType(p)
			 let dtf = propType == typeof(DateTime) || propType == typeof(DateTimeOffset) ? dateFormat : null
			 let fnf = _IsFloatingType(propType) ? floatingNumberFormat : null
			 let gf = val is Guid ? guidFormat : null
			 where dattr?.IsExcluded != true
			 orderby dattr?.GetOrder() ?? ++idx, name
			 select
			 val == null ? null :
			 dtf != null ? ((IFormattable)val).ToString(dateFormat, null) :
			 fnf != null ? ((IFormattable)val).ToString(floatingNumberFormat, null) :
			 gf != null ? ((IFormattable)val).ToString(guidFormat, null) :
			 val.ToString()
		 )
		 .ToArray();


		// -------------------------------------------------------
		static Type? _GetNullableSafeType(PropertyInfo arg) {
			Type argPiType = arg.PropertyType;
			bool argPiTypeIsNullable = argPiType.IsGenericType && argPiType.GetGenericTypeDefinition() == typeof(Nullable<>);

			if (argPiTypeIsNullable)
				return Nullable.GetUnderlyingType(arg.PropertyType);
			else
				return argPiType;
		}

		// -------------------------------------------------------
		static bool _IsFloatingType(Type t) => t == typeof(float) || t == typeof(double) || t == typeof(decimal);
	}

	/// <summary>
	/// Provides string data array that should be ordered and displayed against <see cref="Headers"/> when displaying an implementer in a tabular data format.
	/// <para>By default is implemented against <see cref="Data(object, string, string, string)"/> with default formatters</para>
	/// </summary>
	/// <returns>An array of strings representative of an implementing class</returns>
	public string[] Data() => Data(this);

	/// <summary>
	/// Gets header names represented by a class. The default implementation will use reflection to grab all public instance names that can be
	/// read, and will use the <see cref="TabularDisplayAttribute.Label"/> property, or if null / not present, the <see cref="MemberInfo.Name"/>
	/// <para>Additionally, the <see cref="TabularDisplayAttribute.GetOrder"/> method is used to order headers as specified,
	/// those without (or where null) will begin ordering at <see cref="_INITIALIDX"/>. Sorting is then performed by name (or label) to account
	/// for collisions or group-sorting</para>
	/// </summary>
	/// <returns>An array of strings</returns>
	public string[] Headers() {
		int idx = _INITIALIDX;
		return
			(from p in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
			 let dattr = p.GetCustomAttribute<TabularDisplayAttribute>()
			 let name = dattr?.Label ?? p.Name
			 where dattr?.IsExcluded != true
			 orderby dattr?.GetOrder() ?? ++idx, name
			 select name)
			.ToArray();
	}
}