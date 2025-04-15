namespace Praxis.Web;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Partial class used for providing extension methods; this section for those relating to HttpContext
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Returns the name of a controller without the suffix "controller" as part of the returned string so that it more naturally fits
	/// native url or route paths
	/// <para>(e.g. IndexController -> Index)</para>
	/// </summary>
	/// <param name="controller">The controller to return the name of</param>
	/// <returns>The name of a controller without the 'Controller' string suffix</returns>
	public static string GetRouteName(this Controller controller) {
		string name = controller.GetType().Name;
		return name.EndsWith("Controller") ? name[..^10] : name;
	}

	/// <summary>
	/// Returns the name of a controller without the suffix "controller" as part of the returned string so that it more naturally fits
	/// native url or route paths
	/// <para>(e.g. IndexController -> Index)</para>
	/// </summary>
	/// <typeparam name="T">The type of controller to retrieve a name for</typeparam>
	/// <returns>The name of a controller without the 'Controller' string suffix</returns>
	public static string GetRouteName<T>() where T : Controller {
		string name = typeof(T).Name;
		return name.EndsWith("Controller") ? name[..^10] : name;
	}
}