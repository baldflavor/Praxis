namespace Praxis.Data.Cosmos;

using System;


/// <summary>
/// Attribute that designates that this property may be "undefined" in Cosmos backing data. In this case such properties will not evaluate in an expected manner unless accounted for
/// when the query is executed on the server.
/// As an example, an object with a boolean property, when queried using LINQ-Cosmos, and checking that myObj.MyBooleanProperty=false would not return objects where the property was undefined.
/// This is more of a warning to take care with queries when translating them using LINQ (where the .IsDefined()) method is useful, or when composing SQL style queries in which case
/// the IS_DEFINED() function can be used.
/// It is recommended to include the following in the summary tag of a properties that has this attribute:
/// <para>NOTE: <seealso cref="UndefinedPossibleAttribute"/></para>
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class UndefinedPossibleAttribute : Attribute {
}