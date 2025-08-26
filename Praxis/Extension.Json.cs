namespace Praxis;

using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Uses recursion to walk through the properties of a node (and/or over each element in an array and it's properties) and where a property name matches, the value is returned
	/// </summary>
	/// <param name="node">The node to search on</param>
	/// <param name="name">The name of a property to find for returning values</param>
	/// <returns><see cref="IEnumerable{T}"/> of <see cref="JsonNode"/></returns>
	public static IEnumerable<JsonNode> FindNodeValues(this JsonNode node, string name) {
		JsonValueKind vKind = node.GetValueKind();

		if (vKind == JsonValueKind.Array) {
			foreach (JsonNode? element in node.AsArray()) {
				foreach (JsonNode inner in FindNodeValues(element!, name))
					yield return inner;
			}
		}
		else if (vKind == JsonValueKind.Object) {
			foreach (KeyValuePair<string, JsonNode?> jObj in node.AsObject()) {
				if (jObj.Value is null)
					continue;

				if (jObj.Key == name)
					yield return jObj.Value;

				JsonValueKind innerKind = jObj.Value.GetValueKind();
				if (innerKind is JsonValueKind.Array or JsonValueKind.Object) {
					foreach (JsonNode inner in FindNodeValues(jObj.Value, name))
						yield return inner;
				}
			}
		}
	}
}