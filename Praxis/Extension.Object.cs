namespace Praxis;

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Net;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// The maximum object depth to use when getting properties of objects
	/// </summary>
	public static int MaxFullPropertyDepth { get; set; } = 6;

	/// <summary>
	/// Delimiter between property name -> value output in <see cref="ToStringProperties{T}(T, string?, string?, string[])"/>
	/// </summary>
	public static string ToStringPropertiesDelimiter { get; set; } = Const.CRLF;

	/// <summary>
	/// Format to use for property name -> value output in <see cref="ToStringProperties{T}(T, string?, string?, string[])"/>
	/// </summary>
	public static string ToStringPropertiesFormat { get; set; } = "{0}" + Const.RIGHTARROWHEAD + "{1}";

	/// <summary>
	/// Delimiter to use between rich object property->value or collections when calling <see cref="ToStringRich(object?, bool, string?, string?)"/>
	/// </summary>
	public static string ToStringRichDelimiter { get; set; } = Const.CRLF;

	/// <summary>
	/// Format for rich object property->value or collections when calling <see cref="ToStringRich(object?, bool, string?, string?)"/>
	/// </summary>
	public static string ToStringRichFormat { get; set; } = "[{0}" + Const.BROKENVERTBAR + "{1}]";

	/// <summary>
	/// Returns a single matching attribute value on the specified object or one of it's members, optionally using ancestor inheritance for searching
	/// <para>NOTE: Doesn't work for types</para>
	/// </summary>
	/// <param name="arg">The arg object to retrieve the attribute on</param>
	/// <param name="memberName">Used to retrieve the attribute from the named member on arg rather than arg itself. If not exactly one member is not found, an exception will be thrown</param>
	/// <param name="inherited">Whether to use ancestor inheritance for attribute searching (e.g. on derived classes)</param>
	/// <typeparam name="T">The type of attribute to return and search for</typeparam>
	/// <returns>A matching attribute if found on arg/member, otherwise null</returns>
	/// <exception cref="Exception">The request attribute does not exist on the specified object and method parameters specify to fail on missing attribute</exception>
	/// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found</exception>
	public static T? Attribute<T>(this object arg, string? memberName = null, bool inherited = true) where T : System.Attribute {
		Type type = arg.GetType();

		T? attrib;
		if (memberName == null) {
			if (type.IsEnum)
				return arg.Attribute<T>(arg.ToString(), inherited);
			else
				attrib = type.GetCustomAttribute<T>(inherited);
		}
		else {
			attrib = type.GetMember(memberName).Single().GetCustomAttribute<T>(inherited);
		}

		return attrib;
	}

	/// <summary>
	/// Shallow clones the passed in object source to a new type -- using reflection to property match by name and type.
	/// </summary>
	/// <remarks>
	/// <b>BE AWARE</b> that this may set <c>null</c> values to properties on the destination (such as the case of <c>string</c>) which are not
	/// marked as <i>nullable</i> [<c>?</c>].
	/// </remarks>
	/// <typeparam name="T">The type to clone to.</typeparam>
	/// <param name="source">The source object to clone from.</param>
	/// <returns>An object of type TData with set values that match from source.</returns>
	/// <exception cref="ArgumentException">Source is null.</exception>
	public static T CloneToType<T>(this object source) where T : notnull {
		T destination = Activator.CreateInstance<T>();

		source.PushPropertiesTo(destination);

		return destination;
	}

	/// <summary>
	/// Gets a property info instance given the expression / lambda specified by object type.
	/// </summary>
	/// <typeparam name="T">The type to interrogate</typeparam>
	/// <typeparam name="TProperty">The type of property the expression represents</typeparam>
	/// <param name="obj">Object used for type retrieval / inference</param>
	/// <param name="expPropLambda">Expression/func examined for which property should have it's info returned</param>
	/// <returns>A <see cref="PropertyInfo"/> that matches the property specified in <paramref name="expPropLambda"/></returns>
	/// <exception cref="ArgumentException">Thrown if the expression-lambda does not point to a valid property</exception>
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The object is used for type inference / convenience")]
	public static PropertyInfo GetPropertyInfo<T, TProperty>(this T obj, Expression<Func<T, TProperty>> expPropLambda) where T : class {
		return GetPropertyInfo(expPropLambda);
	}

	/// <summary>
	/// Gets a property info instance given the expression / lambda specified by object type.
	/// </summary>
	/// <typeparam name="T">The type to interrogate</typeparam>
	/// <typeparam name="TProperty">The type of property the expression represents</typeparam>
	/// <param name="obj">Object used for type retrieval / inference</param>
	/// <param name="expPropLambda">Expression/func examined for which property should have it's info returned</param>
	/// <returns>A <see cref="PropertyInfo"/> that matches the property specified in <paramref name="expPropLambda"/></returns>
	/// <exception cref="ArgumentException">Thrown if the expression-lambda does not point to a valid property</exception>
	public static PropertyInfo GetPropertyInfo<T, TProperty>(Expression<Func<T, TProperty>> expPropLambda) where T : class {
		if (expPropLambda.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
			return propertyInfo;

		throw new ArgumentException("The lambda expression does point to a valid property", nameof(expPropLambda));
	}

	/// <summary>
	/// Pushes matching property values from a source object and type to a destination object and type. Properties are matched in a case insensitive fashion.
	/// </summary>
	/// <b>BE AWARE</b> that this may set <c>null</c> values to properties on the destination (such as the case of <c>string</c>) which are not
	/// marked as <i>nullable</i> [<c>?</c>].
	/// </remarks>
	/// <param name="source">Source object to draw from.</param>
	/// <param name="sourceType">Source type taken from source.</param>
	/// <param name="destination">Destination object to push to.</param>
	/// <param name="destinationType">Destination type taken from destination.</param>
	/// <param name="skipProperties">Array of property names to skip when pushing values from one object to the other.</param>
	/// <exception cref="ArgumentException">Source is null or destination is null.</exception>
	public static void PushPropertiesTo<T, K>(this T source, K destination, params string[] skipProperties) where T : notnull where K : notnull {
		var sourceType = source.GetType();
		var destinationType = destination.GetType();

		// Binding flags do not work on these calls other than the access modifiers
		IEnumerable<PropertyInfo> sourceProperties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(pi => pi.CanRead);
		IEnumerable<PropertyInfo> destinationProperties = destinationType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(pi => pi.CanWrite);

		if (skipProperties.Length > 0)
			destinationProperties = destinationProperties.Where(d => !skipProperties.Contains(d.Name));

		foreach (PropertyInfo? destPi in destinationProperties) {
			bool destPiTypeIsNullable = _IsNullableType(destPi, out Type destPiType);

			PropertyInfo? sourcePi = sourceProperties.SingleOrDefault(spi => spi.Name.EqualsOIC(destPi.Name));
			if (sourcePi == null)
				continue;

			bool sourcePiTypeIsNullable = _IsNullableType(sourcePi, out Type sourcePiType);

			if (sourcePiType != destPiType)
				continue;

			if (sourcePiTypeIsNullable && !destPiTypeIsNullable)
				destPi.SetValue(destination, sourcePi.GetValue(source) ?? Activator.CreateInstance(sourcePiType));
			else
				destPi.SetValue(destination, sourcePi.GetValue(source));
		}
	}

	/// <summary>
	/// On the target object, walks through type/base type heirarchy and retrieves all fields (<see cref="Const.BindingFlagsAll"/>) whose type is a 
	/// subclass of <see cref="MulticastDelegate"/>.
	/// <para>For each of those fields the value will be set to <see langword="null"/></para>
	/// </summary>
	/// <param name="arg">Object being targeted</param>
	public static void SetEventHandlersNull(this object arg) {
		Type multicastDelegateType = typeof(MulticastDelegate);
		Type? type = arg.GetType();
		while (type != null) {
			foreach (FieldInfo field in type.GetFields(Const.BindingFlagsAll).Where(f => f.FieldType.IsSubclassOf(multicastDelegateType)))
				field.SetValue(arg, null);

			type = type.BaseType;
		}
	}

	/// <summary>
	/// Serializes the passed object into Json using <see cref="JsonSerializer.Serialize"/> with <see cref="Tools.JsonOptions"/>
	/// </summary>
	/// <param name="arg">Object to serialize into JSON</param>
	/// <returns>A string containing json</returns>
	/// <exception cref="NotSupportedException">Thrown by the serializer when serialization cannot be completed on the passed object</exception>
	public static string ToJson(this object arg) => JsonSerializer.Serialize(arg, Json.Options);

	/// <summary>
	/// Serializes the passed object into a Json Node using <see cref="JsonSerializer.Serialize"/> with <see cref="Tools.JsonOptions"/>
	/// </summary>
	/// <param name="arg">Object to serialize into JSON</param>
	/// <returns>A Json node</returns>
	/// <exception cref="NotSupportedException">Thrown by the serializer when serialization cannot be completed on the passed object</exception>
	public static JsonNode ToJsonNode(this object arg) => JsonSerializer.SerializeToNode(arg, Json.Options) ?? throw new Exception("Could not create a node from the passed argument");

	/// <summary>
	/// Returns a string that contains all of the public, instance, readable property values from an object
	/// <para>If the passed object is <see langword="null"/>, then "null" will be returned</para>
	/// <para>Does not work on structs</para>
	/// </summary>
	/// <remarks>
	/// No longer uses a dictionary to hold onto property per type as this should
	/// </remarks>
	/// <param name="arg">The object to return properties for</param>
	/// <param name="format">A format string desired for displaying each property-value pair on the object (ALT+0155 default separator)</param>
	/// <param name="delimiter">The delimiter string to use between each item</param>
	/// <param name="excludeProperties">Properties to exclude by name</param>
	/// <returns>A string</returns>
	public static string ToStringProperties<T>(this T arg, string? format = default, string? delimiter = default, params string[] excludeProperties) {
		int depth = 0;
		return _ToStringProperties(arg, format, delimiter, ref depth, excludeProperties);
	}

	/// <summary>
	/// Performs validation on an object using data annotations
	/// </summary>
	/// <param name="arg">Object being validated</param>
	/// <param name="validationResults">Out parameter List of validation results that will be filled with any potential validation failures</param>
	/// <param name="propertyName">If passed, will only validate the specified property of the object rather than the object as a whole.</param>
	/// <returns>True if validation passed, otherwise false</returns>
	public static bool TryValidate(this object arg, out List<ValidationResult> validationResults, string? propertyName = null) {
		validationResults = [];

		if (propertyName == null) {
			return Validator.TryValidateObject(arg, new ValidationContext(arg), validationResults, true);
		}
		else {
			return
					Validator.TryValidateProperty(
							arg.GetType().GetProperty(propertyName)!.GetValue(arg),
							new ValidationContext(arg) { MemberName = propertyName },
							validationResults);
		}
	}


	/// <summary>
	/// Performs validation on a arg object. For any fields that do not pass validation, if those properties are read properties,
	/// then their current offending value will be filled into the output list. If they are writeable properties, then they will
	/// be set to their backing CLR default value. If the harsh mode is used, then .Validate is called post scrubbing, and if the
	/// object still contains invalid data, then an Exception will be thrown
	/// </summary>
	/// <param name="arg">Object to perform validation upon</param>
	/// <param name="valresfail">Out list of value tuples describing which properties failed, and their values</param>
	/// <param name="useHarshMode">If true, will call validate on the arg object post scrubbing. This may throw an exception.</param>
	/// <returns>True if the object passed validation, false if validation did not pass and the object was scrubbed</returns>
	public static bool ValidateScrub(this object arg, out List<(string? errorMessage, string memberName, object? failValue)> valresfail, bool useHarshMode = true) {
		valresfail = [];

		if (arg.TryValidate(out List<ValidationResult> valres))
			return true;

		Type type = arg.GetType();
		foreach (ValidationResult vr in valres) {
			foreach (string vrmemb in vr.MemberNames) {
				PropertyInfo? prop = type.GetProperty(vrmemb);
				Type propType = prop!.PropertyType;

				valresfail.Add((vr.ErrorMessage!, vrmemb, prop.CanRead ? prop.GetValue(arg) : null));

				if (prop.CanWrite) {
					object? valueToSet;
					if (propType.IsValueType)
						valueToSet = Activator.CreateInstance(propType);
					else
						valueToSet = null;

					prop.SetValue(arg, valueToSet);
				}
			}
		}

		if (useHarshMode)
			Assert.IsValid(arg);

		return false;
	}

	/// <summary>
	/// Performs validation on the value <paramref name="arg"/> against <see cref="ValidationAttribute"/>s that are present on the <typeparamref name="TProperty"/> of <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T">Type of object that contains the property</typeparam>
	/// <typeparam name="TProperty">The property of the object to obtain validation rules for</typeparam>
	/// <param name="arg">The value being validated</param>
	/// <param name="expPropLambda">Used to obtain the property being examined</param>
	/// <param name="valRes">Filled with <see cref="ValidationResult"/>s for those that did not pass validation. Will be empty if validation has succeeded</param>
	/// <param name="searchInherited">Whether to search ancestors when retrieving validation properties</param>
	/// <returns><see langword="true"/> if <paramref name="arg"/> is valid, otherwise false</returns>
	/// <exception cref="Exception">May occur if the property cannot be interrogated or attributes returned</exception>
	/// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
	public static bool ValidateValueFor<T, TProperty>(this object? arg, Expression<Func<T, TProperty>> expPropLambda, out ICollection<ValidationResult> valRes, bool searchInherited) where T : class {
		PropertyInfo propInfo = GetPropertyInfo(expPropLambda);
		IEnumerable<ValidationAttribute> valAttribs = propInfo.GetCustomAttributes<ValidationAttribute>(searchInherited);
		valRes = [];

		// The validation context does not have to be any appreciable instance; the important detail is the member name otherwise the validation
		// results will have the incorrect name on return
		return Validator.TryValidateValue(arg, new ValidationContext(true) { MemberName = propInfo.Name }, valRes, valAttribs);
	}

	/// <summary>
	/// Creates an instance with state and properties set by utilizing <see cref="Extension.TryValidate(object, out List{ValidationResult}, string?)"/>
	/// to obtain valid / invalid state using data annotations.
	/// </summary>
	/// <param name="arg">Object to check for validity according to data annotations</param>
	/// <param name="valResult">Out parameter of <see cref="ValidationResult"/></param>
	/// <returns>Value that indicates whether <paramref name="arg"/> passed validation</returns>
	public static bool ValidFor(this object arg, out ValidatedResult valResult) {
		bool isValid = arg.TryValidate(out List<ValidationResult> valRes);

		if (isValid)
			valResult = ValidatedResult.Ok();
		else
			valResult = ValidatedResult.Invalid(valRes);

		return isValid;
	}

	/// <summary>
	/// Creates an instance with state and properties set by utilizing <see cref="Extension.TryValidate(object, out List{ValidationResult}, string?)"/>
	/// to obtain valid / invalid state using data annotations.
	/// </summary>
	/// <typeparam name="T">Type of data which may be set against the instance</typeparam>
	/// <param name="arg">Object to check for validity according to data annotations</param>
	/// <param name="valResult">Out parameter of <see cref="ValidationResult"/></param>
	/// <returns>Value that indicates whether <paramref name="arg"/> passed validation</returns>
	public static bool ValidFor<T>(this object arg, out ValidatedResult<T> valResult) {
		bool isValid = arg.TryValidate(out List<ValidationResult> valRes);

		if (isValid)
			valResult = ValidatedResult.Ok<T>();
		else
			valResult = ValidatedResult.Invalid<T>(valRes);

		return isValid;
	}

	/// <summary>
	/// Returns a string representation of the passed arg object; if the object ends up being either an IDictionary
	/// or an IEnumerable then they will be recursively traveled with a ToStrings with useFullPropertyMode=false
	/// </summary>
	/// <param name="arg">The arg object to represent as a string</param>
	/// <param name="depth">Current depth of object traversal</param>
	/// <param name="useFullPropertyMode">Whether to simply use "ToStrings" on non-primitive objects, or to use a full ToStringProperties call. Beware of recursion / stack overflow</param>
	/// <param name="format">Formatter used when an IDictionary is present as arg</param>
	/// <param name="delimiter">Delimiter used when an IDictionary or IEnumerable is present as arg</param>
	/// <returns>A string representation of the passed arg object</returns>
	internal static string ToStringRich(this object? arg, ref int depth, bool useFullPropertyMode, string? format = default, string? delimiter = default) {
		format ??= ToStringRichFormat;
		delimiter ??= ToStringRichDelimiter;

		if (arg == null)
			return Const.NULL;

		Type t = arg.GetType();

		if (arg is string argString)
			return argString;
		else if (t.IsPrimitive || arg is DateTimeOffset)
			return arg.ToString() ?? Const.NULL;
		else if (arg is Guid argGuid)
			return argGuid.ToString("N");
		else if (arg is DateTime argDateTime)
			return new DateTimeOffset(argDateTime).ToString();
		else if (arg is TimeSpan ts)
			return ts.ToString();
		else if (arg is DictionaryEntry de)
			return string.Format(format, de.Key, de.Value!.ToStringRich(ref depth, useFullPropertyMode, format, delimiter));
		else if (arg is IEnumerable enumerable)
			return enumerable.ToStrings(useFullPropertyMode).Join(delimiter);
		else if (arg is FileSystemInfo argFsi)
			return argFsi.ToString();
		else if (arg is Type argType)
			return argType.ToString();
		else if (arg is MethodBase argMethBase)
			return argMethBase.Name;
		else
			return useFullPropertyMode ? _ToStringProperties(arg, format, delimiter, ref depth) ?? Const.NULL : arg.ToString() ?? Const.NULL;
	}

	/// <summary>
	/// Determines whether the passed property info has a backing type that is nullable
	/// </summary>
	/// <param name="arg">Target property info to check</param>
	/// <param name="actualType">The actual CLR type backing / boxed inside a potential nullable type, or the original type if not nullable</param>
	/// <returns>True if the backing type was nullable, otherwise false</returns>
	private static bool _IsNullableType(PropertyInfo arg, out Type actualType) {
		Type argPiType = arg.PropertyType;
		bool argPiTypeIsNullable = argPiType.IsGenericType && argPiType.GetGenericTypeDefinition() == typeof(Nullable<>);

		if (argPiTypeIsNullable)
			actualType = Nullable.GetUnderlyingType(arg.PropertyType) ?? throw new Exception("Unable to get underlycing type of the passed arg");
		else
			actualType = argPiType;

		return argPiTypeIsNullable;
	}

	/// <summary>
	/// Returns a string that contains all of the public, instance, readable property values from an object
	/// <para>If the passed object is <see langword="null"/>, then "null" will be returned</para>
	/// <para>Does not work on structs</para>
	/// </summary>
	/// <remarks>
	/// No longer uses a dictionary to hold onto property per type as this should
	/// </remarks>
	/// <param name="arg">The object to return properties for</param>
	/// <param name="format">A format string desired for displaying each property-value pair on the object (ALT+0155 default separator)</param>
	/// <param name="delimiter">The delimiter string to use between each item</param>
	/// <param name="depth">Current depth of object traversal</param>
	/// <param name="excludeProperties">Properties to exclude by name</param>
	/// <returns>A string</returns>
	private static string _ToStringProperties<T>(T arg, string? format, string? delimiter, ref int depth, params string[] excludeProperties) {
		format ??= ToStringPropertiesFormat;
		delimiter ??= ToStringPropertiesDelimiter;

		if (arg == null)
			return Const.NULL;

		if (depth++ > MaxFullPropertyDepth)
			return string.Format(format, "Max Depth", "Object depth of " + MaxFullPropertyDepth + " exceeded");

		StringBuilder sb = new();

		Type tType = arg.GetType();
		foreach (PropertyInfo pi in tType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => pi.CanRead).OrderBy(pi => pi.Name)) {
			if (excludeProperties != null && excludeProperties.Contains(pi.Name, StringComparer.OrdinalIgnoreCase))
				continue;

			try {
				ParameterInfo[] paramInfos = pi.GetIndexParameters();
				if (paramInfos != null && paramInfos.Length > 0)
					sb.AppendFormat(format, pi.Name, "indexed property");
				else
					sb.AppendFormat(format, pi.Name, pi.GetValue(arg, null)!.ToStringRich(ref depth, true));
			}
			catch (Exception pex) {
				sb.AppendFormat(format, pi.Name, "!!Exception->" + pex.Message);
			}
			finally {
				sb.Append(delimiter);
			}
		}

		if (sb.Length > delimiter.Length)
			sb.Remove(sb.Length - delimiter.Length, delimiter.Length);

		--depth;

		return sb.ToString();
	}
}
