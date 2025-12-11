namespace Praxis;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Sends a request to a remote endpoint and returns an object deserialized from the response.
	/// </summary>
	/// <remarks>
	/// Response from the server is expected to be <b><c>Json</c></b>.
	/// </remarks>
	/// <typeparam name="T">Type to use when deserializing the response from the remote endpoint.</typeparam>
	/// <param name="client">Client used for web request.</param>
	/// <param name="method">Verb/method used during communication.</param>
	/// <param name="baseUri">Base or root address of remote destination.</param>
	/// <param name="relativeUri">Relative portion of remote destination combined with <paramref name="baseUri"/>.</param>
	/// <param name="data">Object sent as either query string parameters or json depending on <paramref name="method"/>.</param>
	/// <param name="headers">Headers to send along with request.</param>
	/// <param name="cTok">Token used for cancellation of asynchronous processes.</param>
	/// <returns><c>Task</c> of <typeparamref name="T"/>.</returns>
	/// <exception cref="HttpRequestException">Thrown if the response does not have a successful status code.</exception>
	public static async Task<T?> Retrieve<T>(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) where T : class {
		using HttpResponseMessage response = await _HttpClientRetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _HttpResponseMessageAssertSuccess(response, cTok).ConfigureAwait(false);

		using Stream stream = await response.Content.ReadAsStreamAsync(cTok).ConfigureAwait(false);
		return await JsonSerializer.DeserializeAsync<T>(stream, Json.Options, cTok).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a request to a remote endpoint and returns the response body as a <c>string</c>.
	/// </summary>
	/// <param name="client">Client used for web request.</param>
	/// <param name="method">Verb/method used during communication.</param>
	/// <param name="baseUri">Base or root address of remote destination.</param>
	/// <param name="relativeUri">Relative portion of remote destination combined with <paramref name="baseUri"/>.</param>
	/// <param name="data">Object sent as either query string parameters or json depending on <paramref name="method"/>.</param>
	/// <param name="headers">Headers to send along with request.</param>
	/// <param name="cTok">Token used for cancellation of asynchronous processes.</param>
	/// <returns><c>Task</c> of <c>string</c>.</returns>
	/// <exception cref="HttpRequestException">Thrown if the response does not have a successful status code.</exception>
	public static async Task<string> Retrieve(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) {
		using HttpResponseMessage response = await _HttpClientRetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _HttpResponseMessageAssertSuccess(response, cTok).ConfigureAwait(false);

		return await response.Content.ReadAsStringAsync(cTok).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a request to a remote enpoint.
	/// </summary>
	/// <remarks>
	/// Does not read the response other than to check that it has a successful status code.
	/// </remarks>
	/// <param name="client">Client used for web request.</param>
	/// <param name="method">Verb/method used during communication.</param>
	/// <param name="baseUri">Base or root address of remote destination.</param>
	/// <param name="relativeUri">Relative portion of remote destination combined with <paramref name="baseUri"/>.</param>
	/// <param name="data">Object sent as either query string parameters or json depending on <paramref name="method"/>.</param>
	/// <param name="headers">Headers to send along with request.</param>
	/// <param name="cTok">Used for cancellation of asynchronous operation.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="HttpRequestException">Thrown if the response does not have a successful status code.</exception>
	public static async Task Transmit(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) {
		using HttpResponseMessage response = await _HttpClientRetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _HttpResponseMessageAssertSuccess(response, cTok).ConfigureAwait(false);
	}


	/// <summary>
	/// Asserts that a response message is in the range of successful status codes.
	/// </summary>
	/// <remarks>
	/// If the status code is not a success, an attempt is made to read the response content as a string and place it in the resulting exception.
	/// </remarks>
	/// <param name="arg">Value to assert against.</param>
	/// <param name="cTok">Used for cancelling asynchronous operation.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="HttpRequestException">Thrown if the response does not have a successful status code.</exception>
	private static async Task _HttpResponseMessageAssertSuccess(HttpResponseMessage arg, CancellationToken cTok) {
		if (!arg.IsSuccessStatusCode) {
			Exception? contentReadException = null;
			string? content;
			try {
				content = arg.Content is null ? null : await arg.Content.ReadAsStringAsync(cTok).ConfigureAwait(false);
			}
			catch (Exception ex) {
				contentReadException = ex;
				content = "Exception reading content. Check the top level exception's .InnerException for detail";
			}

			var hrex = new HttpRequestException("HttpResponse message status does not indicate success", contentReadException);

			hrex.Data[nameof(arg.StatusCode)] = arg.StatusCode;
			hrex.Data[nameof(arg.ReasonPhrase)] = arg.ReasonPhrase;
			hrex.Data[nameof(arg.Content)] = content;

			throw hrex;
		}
	}

	/// <summary>
	/// Retrieves a response message from a remote endpoint.
	/// </summary>
	/// <remarks>
	/// Determines transmission of data, whether as a query string arguments or Json, depending on <paramref name="method"/>.
	/// </remarks>
	/// <param name="client">Client used for web request.</param>
	/// <param name="method">Verb/method used during communication.</param>
	/// <param name="baseUri">Base or root address of remote destination.</param>
	/// <param name="relativeUri">Relative portion of remote destination combined with <paramref name="baseUri"/>.</param>
	/// <param name="data">Object sent as either query string parameters or json depending on <paramref name="method"/>.</param>
	/// <param name="headers">Headers to send along with request.</param>
	/// <param name="cTok">Used for cancellation of asynchronous operation.</param>
	/// <returns><c>Task</c></returns>
	private static async Task<HttpResponseMessage> _HttpClientRetrieveResponse(HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri, object? data, Dictionary<string, string>? headers, CancellationToken cTok) {
		if (method == HttpMethod.Head || method == HttpMethod.Options)
			throw new ArgumentException(method + " is not supported", nameof(method));

		using HttpContent? content = await _ReconstructRelativeUriMakeContent().ConfigureAwait(false);

		cTok.ThrowIfCancellationRequested();

		using HttpRequestMessage request = new(method, new Uri(baseUri, relativeUri));

		if (content != null)
			request.Content = content;

		if (headers != null) {
			foreach (KeyValuePair<string, string> kvpHeader in headers)
				request.Headers.Add(kvpHeader.Key, kvpHeader.Value);
		}

		cTok.ThrowIfCancellationRequested();

		return await client.SendAsync(request, cTok).ConfigureAwait(false);

		/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		 * Reconstructs the relative uri to contain query string fields if this is an Http.Get request with appreciable data
		 * */
		async Task<HttpContent?> _ReconstructRelativeUriMakeContent() {
			if (data != null) {
				// Check to see if this is this IEnumerable KVPs
				var daie = data as IEnumerable<KeyValuePair<string, string?>>;

				if (method == HttpMethod.Get) {
					relativeUri = $"{relativeUri}{(relativeUri?.EndsWith('?') == true ? null : '?')}{await Kvp.ToUriQueryString(daie ?? Kvp.FromObject(data)).ConfigureAwait(false)}";
				}
				else {
					return
						daie is null ?
							new StringContent(JsonSerializer.Serialize(data, Json.Options), Encoding.UTF8, "application/json") :
							new FormUrlEncodedContent(daie);
				}
			}

			return null;
		}
	}
}
