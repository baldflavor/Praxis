namespace Praxis;

using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

public static partial class Json {

	/// <summary>
	/// Class used for converting <see cref="IPEndPoint"/> instances to and from Json
	/// </summary>
	public sealed class IPEndpointJsonConverter : JsonConverter<IPEndPoint> {

		/// <summary>
		/// Reads the value from Json
		/// </summary>
		/// <param name="reader">Reader used during deserialization</param>
		/// <param name="typeToConvert">The type being used for conversion</param>
		/// <param name="options">The options being used during deserialization</param>
		/// <returns>An <see cref="IPEndPoint"/></returns>
		public override IPEndPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			return IPEndPoint.Parse(reader.GetString()!);
		}

		/// <summary>
		/// Writes the value to Json
		/// </summary>
		/// <param name="writer">Writer used during serialization</param>
		/// <param name="toWrite">The object that needs to be written out as Json</param>
		/// <param name="options">The options being used during serialization</param>
		public override void Write(Utf8JsonWriter writer, IPEndPoint toWrite, JsonSerializerOptions options) {
			writer.WriteStringValue(toWrite.ToString());
		}
	}
}