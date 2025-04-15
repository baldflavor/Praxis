namespace Praxis.Net;

using System.Text.Json.Serialization;

/// <summary>
/// Helper class and methods used to get/post to http endpoints that return JSON responses to OAuth2. Cache bust
/// </summary>
/// <remarks>
/// Initializes a new instance of the HttpClientOAuth class.
/// </remarks>
/// <param name="username">Username to use when requesting an OAuth token</param>
/// <param name="password">Password to use when requesting an OAuth token</param>
/// <param name="tokenUri">The uri to use when requesting OAuth tokens</param>
/// <param name="client">HttpClient to use for operations</param>
public class HttpClientOAuth(string username, string password, Uri tokenUri, HttpClient client) {

	/// <summary>
	/// Holds a reference to a SemaphoreSlim class that is used to make sure concurrent requests to retrieval don't double retrieve keys
	/// </summary>
	private readonly SemaphoreSlim _semaphoreslim = new(1, 1);

	/// <summary>
	/// The authorization token that has been claimed and can be sent back to authorize a request
	/// <para>Contains a prefix</para>
	/// </summary>
	private string? _tokenValue;

	/// <summary>
	/// Holds a date and time beyond which the current token value has expired and need to be refreshed
	/// </summary>
	protected DateTimeOffset TokenExpiresAt { get; set; }


	/// <summary>
	/// Returns a list of key value pairs that comprise most common fields needed to get an authorization token
	/// </summary>
	/// <param name="username">The username to use when requesting a token</param>
	/// <param name="password">The password to use when requesting a token</param>
	/// <returns>A list of KeyValuePairs</returns>
	public Dictionary<string, string> GetAuthTokenRequestValues(string username, string password) {
		var toReturn = new Dictionary<string, string> { { "grant_type", "password" } };

		if (!string.IsNullOrWhiteSpace(username))
			toReturn["username"] = username;

		if (!string.IsNullOrWhiteSpace(password))
			toReturn["password"] = password;

		AuthTokenRequestExtra(toReturn);

		return toReturn;
	}

	/// <summary>
	/// Performs a simple request for an AuthToken - if you require more complex methods you can make this request on your own.
	/// <para>Note that the returned access token has: "Bearer " added to the beginning of the returned token per spec</para>
	/// </summary>
	/// <param name="username">Username to send for the token request</param>
	/// <param name="password">Password to send for the token request</param>
	/// <param name="tokenUri">The URI to use when requesting the token</param>
	/// <returns>An AuthTokenResponse</returns>
	public async Task<OAuthTokenResponse> GetOAuthTokenResponse(string username, string password, Uri tokenUri, CancellationToken ctok) {
		OAuthTokenResponse? oAuthTokenResponse = await client.Retrieve<OAuthTokenResponse>(
				HttpMethod.Post,
				tokenUri,
				data: GetAuthTokenRequestValues(username, password),
				ctok: ctok)
				.ConfigureAwait(false);

		oAuthTokenResponse!.AccessToken = "Bearer " + oAuthTokenResponse.AccessToken;

		return oAuthTokenResponse;
	}

	/// <summary>
	/// Requests a web location for a response deserialized from JSON
	/// <para>If headers / a token is needed, a semaphore lock will be used to ensure safety among async calls</para>
	/// </summary>
	/// <typeparam name="T">Type of object to be returned as a result of the send request</typeparam>
	/// <param name="method">Http verb to use for request. ** Head and Options are NOT SUPPORTED **</param>
	/// <param name="baseUri">Base URI (e.g. http://www.myapi.com) to use when making the request</param>
	/// <param name="relativeUri">Relative URI portion (e.g. Vehicles/Ford/GetMakes) to use when making the request</param>
	/// <param name="data">Values to be either sent as part of the query string or posted to the request</param>
	/// <param name="headers">List of headers to send with the request</param>
	/// <returns>A type TData object</returns>
	public async Task<T?> Retrieve<T>(HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken ctok = default) where T : class {
		return await client.Retrieve<T>(method, baseUri, relativeUri, data, await _GetAuthHeaders(headers, ctok).ConfigureAwait(false), ctok).ConfigureAwait(false);
	}

	/// <summary>
	/// Requests a web location for a response read in as a string
	/// <para>If headers / a token is needed, a semaphore lock will be used to ensure safety among async calls</para>
	/// </summary>
	/// <param name="method">Http verb to use for request. ** Head and Options are NOT SUPPORTED **</param>
	/// <param name="baseUri">Base URI (e.g. http://www.myapi.com) to use when making the request</param>
	/// <param name="relativeUri">Relative URI portion (e.g. Vehicles/Ford/GetMakes) to use when making the request</param>
	/// <param name="data">Values to be either sent as part of the query string or posted to the request</param>
	/// <param name="headers">List of headers to send with the request</param>
	/// <returns>A string</returns>
	public async Task<string> Retrieve(HttpMethod method, Uri baseUri, string? relativeUri = default, object? data = default, Dictionary<string, string>? headers = default, CancellationToken ctok = default) {
		return await client.Retrieve(method, baseUri, relativeUri, data, await _GetAuthHeaders(headers, ctok).ConfigureAwait(false), ctok).ConfigureAwait(false);
	}



	/// <summary>
	/// This method allows a derived class to alter the collection of parameters send to the auth token url
	/// </summary>
	/// <param name="arg">Target list of key value pairs that is later sent to an token endpoint</param>
	protected virtual void AuthTokenRequestExtra(Dictionary<string, string> arg) { }

	/// <summary>
	/// Calculates when an auth token should be regarded as expired
	/// </summary>
	/// <param name="oAuthTokenResponse">A token response that can be used for calculating expiration where appropriate</param>
	protected virtual void CalculateTokenExpiration(OAuthTokenResponse oAuthTokenResponse) {
		this.TokenExpiresAt = oAuthTokenResponse.Expires.AddSeconds(-30);
	}

	/// <summary>
	/// This method allows derived classes to add / alter headers that are sent with every request to the Send/SendObject methods.
	/// Do note that internally, an 'Authorization' key and value will be added to this dictionary, so beware of collisions
	/// </summary>
	/// <param name="arg">Header dictionary to alter</param>
	protected virtual void CommonRequestHeaders(Dictionary<string, string> arg) {
	}

	/// <summary>
	/// Adds a token to the request header; if a token is needed then one is retrieved internally by the method
	/// </summary>
	/// <param name="headers">Existing headers to accont for when making the new ones</param>
	/// <returns>An awaitable task</returns>
	private async Task<Dictionary<string, string>> _GetAuthHeaders(Dictionary<string, string>? headers, CancellationToken ctok) {
		headers ??= [];

		// Uses a technique similar to a "double lock check" - and only uses the semaphore wait when a token is actually needed
		if (DateTimeOffset.UtcNow >= this.TokenExpiresAt) {
			try {
				await _semaphoreslim.WaitAsync(ctok).ConfigureAwait(false);
				if (DateTimeOffset.UtcNow >= this.TokenExpiresAt) {
					OAuthTokenResponse oAuthTokenResponse = await GetOAuthTokenResponse(username, password, tokenUri, ctok).ConfigureAwait(false);
					CalculateTokenExpiration(oAuthTokenResponse);
					_tokenValue = oAuthTokenResponse.AccessToken;
				}
			}
			finally {
				_semaphoreslim.Release();
			}
		}

		CommonRequestHeaders(headers);
		headers["Authorization"] = _tokenValue!;
		return headers;
	}



	/// <summary>
	/// Represents a response from an OAuth token request
	/// </summary>
	public class OAuthTokenResponse {

		/// <summary>
		/// Initializes a new instance of the OAuthTokenResponse class.
		/// </summary>
		public OAuthTokenResponse() {
		}

		/// <summary>
		/// Gets or sets the access token returned by the request
		/// </summary>
		[JsonPropertyName("access_token")]
		public required string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets when the token has expired
		/// </summary>
		[JsonPropertyName(".expires")]
		public DateTimeOffset Expires { get; set; }

		/// <summary>
		/// Gets or sets the number of seconds from receiving the token upon which is will have expired
		/// </summary>
		[JsonPropertyName("expires_in")]
		public int ExpiresIn { get; set; }

		/// <summary>
		/// Gets or sets when the token was issued
		/// </summary>
		[JsonPropertyName(".issued")]
		public required DateTimeOffset Issued { get; set; }

		/// <summary>
		/// Gets or sets the type of token returned
		/// </summary>
		[JsonPropertyName("token_type")]
		public required string TokenType { get; set; }
	}
}