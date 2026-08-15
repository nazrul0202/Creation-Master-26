using System.Collections;
using System.Windows.Forms;

namespace FifaLibrary;

public class ListViewItemComparer : IComparer
{
	private int col;

	private SortOrder m_SortOrder;

	public ListViewItemComparer()
	{
		col = 0;
	}

	public ListViewItemComparer(int column, SortOrder sortOrder)
	{
		col = column;
		m_SortOrder = sortOrder;
	}

	public int Compare(object x, object y)
	{
		if (m_SortOrder == SortOrder.Ascending)
		{
			return string.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
		}
		return string.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);
	}
}
