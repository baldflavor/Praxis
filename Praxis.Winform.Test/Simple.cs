namespace Praxis.Winform.Test;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class Simple : Form {
	public Simple() {
		InitializeComponent();
		var initMethods = GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(m => m.Name.StartsWith("_Init"));
		foreach (var method in initMethods) {
			method.Invoke(this, null);
		}
	}


	
	private void _InitNLogViewerButton() {
		_nLogViewerButton.Click += (_, _) => {
			if (_ChooseDir() is string dir) {
				new Praxis.WinForm.NLogViewer.NLogViewerForm(
					dir,
					5000,
					TimeSpan.FromMilliseconds(1000),
					TimeSpan.FromMilliseconds(1650))
				.Show();
			}
		};

		string? _ChooseDir() {
			using var fbd = new FolderBrowserDialog() { UseDescriptionForTitle = true, Description = "Choose a Folder of NLog Json Files" };
			return fbd.ShowDialog() == DialogResult.OK ? fbd.SelectedPath : null;
		}
	}
}
