namespace Praxis;


using System.Text;
using System.Xml.Linq;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Returns the string value of a node from a parent node
	/// </summary>
	/// <param name="parent">Parent node to search</param>
	/// <param name="elementName">Sub element's value to return as a string</param>
	/// <returns>A string</returns>
	public static string? ChildString(this XElement parent, string elementName) => (string?)parent.Element(elementName);

	/// <summary>
	/// Returns the string value of a node from a parent node
	/// </summary>
	/// <param name="parent">Parent node to search</param>
	/// <param name="elementName">Sub element's value to return as a string</param>
	/// <returns>A string</returns>
	public static string? ChildString(this XElement parent, XName elementName) => (string?)parent.Element(elementName);

	/// <summary>
	/// Retrieves both the name of an element and the element's value as a string from a parent
	/// </summary>
	/// <param name="parent">Parent node to search</param>
	/// <param name="elementName">Sub element's name</param>
	/// <returns>A KeyValuePair</returns>
	public static KeyValuePair<string, string?> ChildStringKvp(this XElement parent, string elementName) => KeyValuePair.Create(elementName, (string?)parent.Element(elementName));

	/// <summary>
	/// Retrieves both the name of an element and the element's value as a string from a parent
	/// </summary>
	/// <param name="parent">Parent node to search</param>
	/// <param name="elementName">Sub element's name</param>
	/// <returns>A KeyValuePair</returns>
	public static KeyValuePair<string, string?> ChildStringKvp(this XElement parent, XName elementName) => KeyValuePair.Create(elementName.LocalName, (string?)parent.Element(elementName));

	/// <summary>
	/// Clones the arg XElement by calling the constructor of a new XElement with <paramref name="arg"/> passed into the constructor
	/// </summary>
	/// <param name="arg">XElement to be cloned</param>
	/// <returns>A cloned XElement</returns>
	public static XElement Clone(this XElement arg) => new(arg);

	/// <summary>
	/// Will save an xml document with a declaration tag.
	/// </summary>
	/// <param name="arg">Target xelement to write out</param>
	/// <param name="saveOptions">Options for preserving whitespace</param>
	/// <returns></returns>
	public static string ToStringWithDeclaration(this XElement arg, SaveOptions saveOptions = SaveOptions.DisableFormatting) {
		using MemoryStream ms = new();
		using StreamWriter sw = new(ms, Encoding.UTF8);
		arg.Save(sw, saveOptions);
		ms.Position = 0;
		using var sr = new StreamReader(ms);
		return sr.ReadToEnd();
	}

	/// <summary>
	/// Given an XNamespace, use the name of an element to create a qualified element name
	/// </summary>
	/// <param name="arg">XNamespace to use as the source for qualification</param>
	/// <param name="elementName">The name of an element to qualify</param>
	/// <returns>An <see cref="XName"/> used for retrieving elements</returns>
	public static XName With(this XNamespace arg, string elementName) => arg + elementName;
}