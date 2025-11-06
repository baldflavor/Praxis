namespace Praxis.WinForm;

using System.Text;
using System.Windows.Forms;

// Extension methods for treeviews / nodes
public static partial class Extension {

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
	/// Renders a node and its recursive children as indented strings.
	/// </summary>
	/// <param name="node">Value to use as a source for rendered output.</param>
	/// <param name="bullet">Character to use for nested children delineation.</param>
	/// <param name="indentSize">Number of spaces to indent for nested child values.</param>
	/// <returns></returns>
	public static string ToIndentedRecursiveString(this TreeNode node, char bullet = Const.BULLET, int indentSize = 2) {
		var sb = new StringBuilder();
		_RenderNode(node, sb, bullet, 0, indentSize);
		return sb.ToString();

		static void _RenderNode(TreeNode node, StringBuilder sb, char bullet, int depth, int indentSize) {
			sb.AppendLine($"{new string(' ', depth * indentSize)}{Const.BULLET} {node.Text}");

			foreach (TreeNode child in node.Nodes)
				_RenderNode(child, sb, bullet, depth + 1, indentSize);
		}
	}

	/// <summary>
	/// Performs an action on a target <see cref="TreeNode"/> and all of its descendants.
	/// </summary>
	/// <param name="node">The <see cref="TreeNode"/> to use as an operational target and a source of child nodes</param>
	/// <param name="action">An action to take on each <see cref="TreeNode"/></param>
	public static void SelfAndDescendants(this TreeNode node, Action<TreeNode> action) {
		action(node);

		foreach (TreeNode child in node.Nodes)
			child.SelfAndDescendants(action);
	}

	/// <summary>
	/// Sets both the <see cref="TreeNode.ImageIndex"/> and <see cref="TreeNode.SelectedImageIndex"/> to the supplied
	/// value on <paramref name="node"/>
	/// </summary>
	/// <param name="node">TreeNode being assigned values to</param>
	/// <param name="index">The index to assign the values to</param>
	public static void SetImageIndexes(this TreeNode node, int index) {
		node.ImageIndex = index;
		node.SelectedImageIndex = index;
	}
}
