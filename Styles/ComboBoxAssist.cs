using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SUI;

/// <summary>ComboBox 的可编辑输入区滚轮选择行为。</summary>
public static class ComboBoxAssist
{
    public static readonly DependencyProperty EnableEditableMouseWheelProperty =
        DependencyProperty.RegisterAttached("EnableEditableMouseWheel", typeof(bool),
            typeof(ComboBoxAssist), new PropertyMetadata(false, OnEnableChanged));

    public static bool GetEnableEditableMouseWheel(DependencyObject element) =>
        (bool)element.GetValue(EnableEditableMouseWheelProperty);

    public static void SetEnableEditableMouseWheel(DependencyObject element, bool value) =>
        element.SetValue(EnableEditableMouseWheelProperty, value);

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox combo) return;
        if ((bool)e.NewValue) combo.PreviewMouseWheel += OnMouseWheel;
        else combo.PreviewMouseWheel -= OnMouseWheel;
    }

    private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var combo = (ComboBox)sender;
        if (!combo.IsEnabled || !combo.IsEditable || combo.IsDropDownOpen || e.Delta == 0)
            return;

        var editor = combo.Template?.FindName("PART_EditableTextBox", combo) as TextBox;
        if (editor == null || !editor.IsMouseOver) return;

        // Consume even at the boundary so the containing page does not jump.
        e.Handled = true;
        int step = e.Delta > 0 ? -1 : 1;
        int index = combo.SelectedIndex;
        if (index < 0) index = step > 0 ? -1 : combo.Items.Count;
        for (index += step; index >= 0 && index < combo.Items.Count; index += step)
        {
            var container = combo.ItemContainerGenerator.ContainerFromIndex(index) as ComboBoxItem;
            var item = combo.Items[index] as UIElement;
            if ((container != null && !container.IsEnabled) || (item != null && !item.IsEnabled))
                continue;
            combo.SetCurrentValue(ComboBox.SelectedIndexProperty, index);
            editor.SelectAll();
            break;
        }
    }
}
