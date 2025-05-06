namespace Praxis.Net;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class HttpRequest {

	public static async Task<T?> Retrieve<T>(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) where T : class {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _AssertSuccess(response, cTok).ConfigureAwait(false);

		using Stream stream = await response.Content.ReadAsStreamAsync(cTok).ConfigureAwait(false);
		return await JsonSerializer.DeserializeAsync<T>(stream, Json.Options, cTok).ConfigureAwait(false);
	}

	public static async Task<string> Retrieve(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _AssertSuccess(response, cTok).ConfigureAwait(false);

		return await response.Content.ReadAsStringAsync(cTok).ConfigureAwait(false);
	}

	public static async Task Transmit(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken cTok = default) {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, cTok).ConfigureAwait(false);
		await _AssertSuccess(response, cTok).ConfigureAwait(false);
	}


	private static async Task _AssertSuccess(HttpResponseMessage arg, CancellationToken cTok) {
		if (!arg.IsSuccessStatusCode) {
			Exception? contentReadException = null;
			string? content;
			try {
				content = arg.Content == null ? null : await arg.Content.ReadAsStringAsync(cTok).ConfigureAwait(false);
			}
			catch (Exception ex) {
				contentReadException = ex;
				content = "Exception reading content. Check the top level exception's .InnerException for detail";
			}

			var hrex = new HttpRequestException("HttpResponse message status does not indicate success", contentReadException);

			hrex.Data["StatusCode"] = arg.StatusCode;
			hrex.Data["ReasonPhrase"] = arg.ReasonPhrase;
			hrex.Data["Content"] = content;

			throw hrex;
		}
	}

	private static async Task<HttpResponseMessage> _RetrieveResponse(HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri, object? data, Dictionary<string, string>? headers, CancellationToken cTok) {
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
					relativeUri = new StringBuilder(relativeUri).Append(await Kvp.ToUriQueryString(daie ?? Kvp.FromObject(data)).ConfigureAwait(false)).ToString();
				}
				else {
					return
						daie == null ?
							new StringContent(JsonSerializer.Serialize(data, Json.Options), Encoding.UTF8, "application/json") :
							new FormUrlEncodedContent(daie);
				}
			}

			return null;
		}
	}
}