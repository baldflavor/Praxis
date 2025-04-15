namespace Praxis;

using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Provides static methods for working with KeyValuePair[string, string] objects
/// </summary>
public static class Kvp {

	/// <summary>
	/// Type definition for IEnumerable
	/// </summary>
	private static readonly Type _iEnumerableType = typeof(IEnumerable);

	/// <summary>
	/// Type definition for string
	/// </summary>
	private static readonly Type _stringType = typeof(string);

	/// <summary>
	/// Simply allows one to use a shortened method of making an array of KeyValuePairs through the params function rather than having to create a new array in calling / client code
	/// </summary>
	/// <param name="arg">Params array of arg key value pairs to simply return</param>
	/// <returns>An array of KeyValuePair objects</returns>
	public static KeyValuePair<string, string>[] Array(params KeyValuePair<string, string>[] arg) => arg;

	/// <summary>
	/// Creates a key value pair that's suitable as a basic authorization header to be sent along with a request
	/// </summary>
	/// <param name="user">The "user" portion of the basic auth header</param>
	/// <param name="pass">The "password" portion of the basic auth header</param>
	/// <returns>A KeyValuePair</returns>
	public static KeyValuePair<string, string> BasicAuth(string user, string pass) => KeyValuePair.Create("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}")!;

	/// <summary>
	/// Creates an array of KeyValuePairs representing top level public, readable properties using the Name and .ToString value, respectively, for each
	/// <para>Will **IGNORE** arg properties that are Collection types</para>
	/// </summary>
	/// <param name="arg">Target object to retrieve values for</param>
	/// <param name="nameFormat">Format string to use </param>
	/// <returns>An array of KeyValuePair with the propertyname and value.ToString() respectively</returns>
	public static KeyValuePair<string, string?>[] FromObject(object arg, string? nameFormat = null) {
		return
			arg
				.GetType()
				.GetRuntimeProperties()
				.Where(pi => pi.CanRead && (pi.PropertyType == _stringType || !_iEnumerableType.IsAssignableFrom(pi.PropertyType)))
				.Select(pi => KeyValuePair.Create(string.IsNullOrWhiteSpace(nameFormat) ? pi.Name : string.Format(nameFormat, pi.Name), pi.GetValue(arg)?.ToString()))
				.ToArray();
	}


	/// <summary>
	/// Uses key value pairs to create an encoded query string
	/// <para>includes a preceding ?</para>
	/// </summary>
	/// <param name="arg">Target parameters to use. If null or empty, the method returns a null result</param>
	/// <returns>A query string; null if no parameters</returns>
	public static async Task<string?> ToUriQueryString(IEnumerable<KeyValuePair<string, string?>> arg) {
		if (arg == null || !arg.Any())
			return null;

		using var content = new FormUrlEncodedContent(arg);

		return
			new StringBuilder("?")
			.Append(await content.ReadAsStringAsync().ConfigureAwait(false))
			.ToString();
	}
}