namespace Praxis.WinForm;

using System.Windows.Forms;

// Extension methods for treeviews / nodes

internal static partial class Extension {

	/// <summary>
	/// Gets the recursive parent <see cref="TreeNode"/> of the node passed in by <paramref name="arg"/>. If the argument has no parent then it will be returned.
	/// </summary>
	/// <param name="arg">Target <see cref="TreeNode"/> to use for finding a top level <see cref="TreeNode"/></param>
	/// <returns>A <see cref="TreeNode"/></returns>
	public static TreeNode GetTopParent(this TreeNode arg) {
		while (arg.Parent != null)
			arg = arg.Parent;

		return arg;
	}

	/// <summary>
	/// Performs an action on a target <see cref="TreeNode"/> and all of its descendants.
	/// </summary>
	/// <param name="arg">The <see cref="TreeNode"/> to use as an operational target and a source of child nodes</param>
	/// <param name="action">An action to take on each <see cref="TreeNode"/></param>
	public static void SelfAndDescendants(this TreeNode arg, Action<TreeNode> action) {
		action(arg);

		foreach (TreeNode child in arg.Nodes)
			child.SelfAndDescendants(action);
	}

	/// <summary>
	/// Sets both the <see cref="TreeNode.ImageIndex"/> and <see cref="TreeNode.SelectedImageIndex"/> to the supplied
	/// value on <paramref name="arg"/>
	/// </summary>
	/// <param name="arg">TreeNode being assigned values to</param>
	/// <param name="index">The index to assign the values to</param>
	public static void SetImageIndexes(this TreeNode arg, int index) {
		arg.ImageIndex = index;
		arg.SelectedImageIndex = index;
	}
}