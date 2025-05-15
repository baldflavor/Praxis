namespace Praxis.WinForm;

using System.ComponentModel;

// Extension methods for invoking UI necessary operations

public static partial class Extension {

	/// <summary>
	/// Invoke code when required
	/// </summary>
	/// <param name="isi">The object to use for synchronization and invokation</param>
	/// <param name="action">The action to perform</param>
	public static void BeginInvokeIfRequired(this ISynchronizeInvoke isi, MethodInvoker action) {
		if (isi.InvokeRequired) {
			isi.BeginInvoke(action, [isi]);
		}
		else {
			action();
		}
	}

	/// <summary>
	/// Invoke code when required while returning a value
	/// </summary>
	/// <typeparam name="T">The type of the return value</typeparam>
	/// <param name="isi">The object to use for synchronization and invokation</param>
	/// <param name="function">The function to execute</param>
	/// <returns>A value of <typeparamref name="T"/> that may be null </returns>
	public static T? BeginInvokeIfRequiredReturn<T>(this ISynchronizeInvoke isi, Func<T> function) {
		if (isi.InvokeRequired) {
			return (T?)isi.BeginInvoke(function, null);
		}
		else {
			return function();
		}
	}
}