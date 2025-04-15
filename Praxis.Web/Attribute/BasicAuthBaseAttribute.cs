namespace Praxis.Web.Attribute;

using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Base class used for basic authentication implementation
/// </summary>
public abstract class BasicAuthBaseAttribute : ActionFilterAttribute {

	/// <summary>
	/// Gets or sets the realm (or segment) of the site that needs credentials for authorization
	/// </summary>
	public required string Realm {
		get; set;
	}

	/// <summary>
	/// Fills in proper response for a forbidden request
	/// </summary>
	/// <param name="filterContext">ActionExecutingContext to use for response alteration</param>
	/// <param name="headers">Any extra headers that should be </param>
	public static async Task ForbiddenResponse(ActionExecutingContext filterContext, IHeaderDictionary? headers = null) {
		HttpResponse res = filterContext.HttpContext.Response;
		res.Clear();
		res.StatusCode = 403;
		filterContext.Result = new EmptyResult();

		if (headers != null) {
			foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in headers) {
				res.Headers[header.Key] = header.Value;
			}
		}

		await res.CompleteAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Code that executs when an action is executing
	/// </summary>
	/// <param name="filterContext">The current (request) context that the filter was applied to</param>
	public override async void OnActionExecuting(ActionExecutingContext filterContext) {
		ArgumentNullException.ThrowIfNull(filterContext.HttpContext);

		string? auth = filterContext.HttpContext.Request.Headers.Authorization.FirstOrDefault();

		if (string.IsNullOrWhiteSpace(auth)) {
			HttpResponse res = filterContext.HttpContext.Response;
			res.Clear();
			res.Headers.WWWAuthenticate = $"Basic realm=\"{this.Realm}\"";
			res.StatusCode = 401;
			filterContext.Result = new EmptyResult(); // Need this to prevent the action method from executing
			await res.CompleteAsync().ConfigureAwait(false);
			return;
		}

		string user;
		string pass;
		IHeaderDictionary nvcHeaders = filterContext.HttpContext.Request.Headers;
		try {
			// Auth string will come in as "Basic <credentials>" hence the need for the substring
			string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(auth[6..])).Split(':');

			//Up to first semi-colon will be used as the username
			user = credentials[0];

			//Everything else is the password
			pass = string.Join(":", credentials.Where((r, i) => i > 0));

			if (!ValidateCredentials(filterContext, nvcHeaders, pass, filterContext.HttpContext.Request.QueryString, user)) {
				LogAuthFailure(
					null,
					nvcHeaders,
					filterContext!.HttpContext!.GetIPEndpoint()?.ToString(),
					filterContext.HttpContext.Request.Path.Value);

				await ForbiddenResponse(filterContext).ConfigureAwait(false);
			}
		}
		catch (Exception ex) {
			LogAuthFailure(
				ex,
				nvcHeaders,
				filterContext.HttpContext.GetIPEndpoint()?.ToString(),
				filterContext.HttpContext.Request.Path.Value);

			await ForbiddenResponse(filterContext).ConfigureAwait(false);
			return;
		}
	}


	/// <summary>
	/// Logging used when critical errors occurr during the authorization failure
	/// </summary>
	/// <param name="ex">Exception to be logged indicating authorization failure. If NULL, then the failure is the result of invalid credentials and not any recorded error in parsing or retrieving credentials</param>
	/// <param name="headers">Headers that came with the original request</param>
	/// <param name="ipAddress">The originating IP address of the request</param>
	/// <param name="rawUrl">The raw url being requested</param>
	protected abstract void LogAuthFailure(Exception? ex, IHeaderDictionary? headers, string? ipAddress, string? rawUrl);

	/// <summary>
	/// Used to validate the credentials that have been supplied to ensure the requestor can access the specified resource
	/// <para>Allow exceptions to fall out of this method</para>
	/// <para>Returning false will call the LogAuthFailure method</para>
	/// </summary>
	/// <param name="filterContext">Current executing context</param>
	/// <param name="headers">The headers passed in by the request</param>
	/// <param name="pass">The password portion of the passed credentials</param>
	/// <param name="queryString">The querystring passed in by the request</param>
	/// <param name="user">The user portion of the passed credentials</param>
	/// <returns>True if the credentials are valid, false if they are not</returns>
	protected abstract bool ValidateCredentials(ActionExecutingContext filterContext, IHeaderDictionary headers, string pass, QueryString queryString, string user);
}