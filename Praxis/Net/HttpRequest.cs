namespace Praxis.Net;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class HttpRequest {

	public static async Task<T?> Retrieve<T>(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken ctok = default) where T : class {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, ctok).ConfigureAwait(false);
		await _AssertSuccess(response, ctok).ConfigureAwait(false);

		using Stream stream = await response.Content.ReadAsStreamAsync(ctok).ConfigureAwait(false);
		return await JsonSerializer.DeserializeAsync<T>(stream, Json.Options, ctok).ConfigureAwait(false);
	}

	public static async Task<string> Retrieve(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken ctok = default) {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, ctok).ConfigureAwait(false);
		await _AssertSuccess(response, ctok).ConfigureAwait(false);

		return await response.Content.ReadAsStringAsync(ctok).ConfigureAwait(false);
	}

	public static async Task Transmit(this HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken ctok = default) {
		using HttpResponseMessage response = await _RetrieveResponse(client, method, baseUri, relativeUri, data, headers, ctok).ConfigureAwait(false);
		await _AssertSuccess(response, ctok).ConfigureAwait(false);
	}


	private static async Task _AssertSuccess(HttpResponseMessage arg, CancellationToken ctok) {
		if (!arg.IsSuccessStatusCode) {
			Exception? contentReadException = null;
			string? content;
			try {
				content = arg.Content == null ? null : await arg.Content.ReadAsStringAsync(ctok).ConfigureAwait(false);
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

	private static async Task<HttpResponseMessage> _RetrieveResponse(HttpClient client, HttpMethod method, Uri baseUri, string? relativeUri, object? data, Dictionary<string, string>? headers, CancellationToken ctok) {
		if (method == HttpMethod.Head || method == HttpMethod.Options)
			throw new ArgumentException(method + " is not supported", nameof(method));

		using HttpContent? content = await _ReconstructRelativeUriMakeContent().ConfigureAwait(false);

		ctok.ThrowIfCancellationRequested();

		using HttpRequestMessage request = new(method, new Uri(baseUri, relativeUri));

		if (content != null)
			request.Content = content;

		if (headers != null) {
			foreach (KeyValuePair<string, string> kvpHeader in headers)
				request.Headers.Add(kvpHeader.Key, kvpHeader.Value);
		}

		ctok.ThrowIfCancellationRequested();

		return await client.SendAsync(request, ctok).ConfigureAwait(false);

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