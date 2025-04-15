namespace Praxis.Web;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Partial class used for providing extension methods; this section for those relating to HttpContext
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Gets the IP address for the currently executing request context. First, headers are examined for the existence of a
	/// "X-FORWARDED-FOR" header, and if an appreciable value is present, this value is returned. Otherwise the UserHostAddress is returned.
	/// </summary>
	/// <param name="context">Target HttpContext to use for operation</param>
	/// <param name="replacementIpAddress">IP address to use as a replacement for when a local IP address is found</param>
	/// <returns>A string representing the remote client's IP address</returns>
	public static System.Net.IPEndPoint? GetIPEndpoint(this HttpContext context, string? replacementIpAddress = null) {
		if (context.Request == null)
			return null;

		string? ipAddress = context.Request?.Headers?["X-FORWARDED-FOR"];

		if (string.IsNullOrWhiteSpace(ipAddress))
			ipAddress = context.Connection?.RemoteIpAddress?.ToString();

		if (replacementIpAddress != null && (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress?.StartsWith("10.") == true))
			ipAddress = replacementIpAddress;

		if (System.Net.IPEndPoint.TryParse(ipAddress!, out System.Net.IPEndPoint? toReturn))
			return toReturn;
		else
			return null;
	}

	/// <summary>
	/// Reads out the request content body as a string
	/// </summary>
	/// <param name="context"><see cref="HttpRequestBase"/> from which to read the content body</param>
	/// <returns>String representing the content body of the incoming request</returns>
	public static async Task<string?> GetRequestContent(this HttpContext context) {
		try {
			Stream reqStream = context.Request.BodyReader.AsStream();

			if (!reqStream.CanRead)
				return null;

			using var sr = new StreamReader(reqStream);
			return await sr.ReadToEndAsync().ConfigureAwait(false);
		}
		catch (Exception ex) {
			return $"BROKEN: {ex.Message}";
		}
	}
}