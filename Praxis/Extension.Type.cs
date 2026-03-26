namespace Praxis;

using System.Reflection;

public static partial class Extension {
	/// <summary>
	/// Gets field information on those which can be disposed that are non-public instance fields of this type and its <c>BaseType</c>s.
	/// </summary>
	/// <remarks>
	/// Be careful on timing / ordering of the calls, as this does not allow one to control the order in which objects are disposed, which may cause side effects.
	/// </remarks>
	/// <typeparam name="T">Used for retrieval of fields using reflection.</typeparam>
	/// <returns>Reflective field information of IDisposable types.</returns>
	public static IEnumerable<FieldInfo> GetDisposeFields<T>() => typeof(T).GetDisposeFields();

	/// <summary>
	/// Gets field information on those which can be disposed that are non-public instance fields of this type and its <c>BaseType</c>s.
	/// </summary>
	/// <remarks>
	/// Be careful on timing / ordering of the calls, as this does not allow one to control the order in which objects are disposed, which may cause side effects.
	/// </remarks>
	/// <param name="arg">Type for retrieving fields.</param>
	/// <returns>Reflective field information of IDisposable types.</returns>
	public static IEnumerable<FieldInfo> GetDisposeFields(this Type arg) {
		Type? cType = arg;
		IEnumerable<FieldInfo> toReturn = [];

		while (cType is not null) {
			toReturn = toReturn.Concat(_GetByType(cType));
			cType = cType.BaseType;
		}

		return toReturn;

		static IEnumerable<FieldInfo> _GetByType(Type type) {
			return type
					.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(f => f.FieldType.IsAssignableTo(typeof(IDisposable)));
		}
	}

	/// <summary>
	/// Returns whether the type in question represents a type that would be considered to be a kind of number.
	/// </summary>
	/// <param name="t">Type to check.</param>
	/// <returns><c>true</c> if a kind of numeric type, otherwise <c>false</c>.</returns>
	public static bool IsNumericKind(this Type t) {
		if (t.IsEnum)
			return false;

		return Type.GetTypeCode(t) switch {
			TypeCode.Boolean or
			TypeCode.Char or
			 TypeCode.DateTime or
			 TypeCode.Empty or
				TypeCode.Object or
				 TypeCode.String => false,
			_ => true
		};
	}
}
