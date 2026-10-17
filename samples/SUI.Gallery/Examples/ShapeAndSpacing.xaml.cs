using System.Windows.Controls;

namespace SuiExamples;

public partial class ShapeAndSpacing : UserControl
{
    public ShapeAndSpacing()
    {
        InitializeComponent();
        Spacing.ItemsSource = Enumerable.Range(1, 4).Select(i =>
        {
            var key = $"SUI.Space.{i}";
            var value = (double)FindResource(key);
            return new { Label = $"{value:0} px", Value = value };
        }).ToList();
    }
}
