using System.Windows.Controls;

namespace SuiExamples;

public partial class ListAndScroll : UserControl
{
    public ListAndScroll() => InitializeComponent();

    private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectionSummary is not null)
            SelectionSummary.Text = $"已选择 {Choices.SelectedItems.Count} 个角色";
    }
}
