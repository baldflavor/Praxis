namespace Praxis.WinForm;

/// <summary>
/// Used for comparing list view items / subitems when sorting by a header click on a <c>ListView</c>.
/// </summary>
/// <param name="colIndex">Index of the column that was clicked on a <c>ListView</c>.</param>
public class ListViewItemComparer(int colIndex) : System.Collections.IComparer {

	/// <summary>
	/// Index of the column being targeted.
	/// </summary>
	/// <remarks>
	/// Can be used when checking for an existing <see cref="ListView.ListViewItemSorter"/> and whether or not to assign a new comparer for a different
	/// column, or to change the direction (and re-sort) this instance.
	/// </remarks>
	public int ColIndex => colIndex;

	/// <summary>
	/// Gets or sets a value indicating whether the comparison / sorting should be ascending or descending.
	/// </summary>
	public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

	/// <inheritdoc/>
	public int Compare(object? x, object? y) {
		string sX = ((ListViewItem)x.IsNotNull()).SubItems[colIndex].Text;
		string sY = ((ListViewItem)y.IsNotNull()).SubItems[colIndex].Text;

		if (this.SortOrder == SortOrder.Ascending)
			return string.Compare(sX, sY, StringComparison.OrdinalIgnoreCase);
		else
			return string.Compare(sY, sX, StringComparison.OrdinalIgnoreCase);
	}
}
