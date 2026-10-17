using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SuiExamples;

public partial class DeviceWorkspace : UserControl
{
    private readonly ICollectionView _view;
    public DeviceWorkspace()
    {
        InitializeComponent();
        Tree.ItemsSource = new[]
        {
            new Node("全部设备", "200", new Node("在线设备", "134"), new Node("离线设备", "66")),
            new Node("项目资料", "", new Node("README.md", ""), new Node("使用手册.md", "")),
        };
        var devices = Enumerable.Range(1, 200).Select(i => new Row(i, $"工作站 {i:D3}", i % 3 == 0 ? "离线" : "在线", i % 3 == 0 ? 0 : (i * 7.3) % 100)).ToList();
        _view = CollectionViewSource.GetDefaultView(devices);
        _view.Filter = Matches;
        Grid.ItemsSource = _view;
        Grid.SelectedIndex = 0;
        UpdateCount();
    }
    private bool Matches(object item)
    {
        var row = (Row)item;
        var query = Filter.Text.Trim();
        var status = (StatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
        return (row.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || row.Id.ToString().Contains(query))
            && (StatusFilter.SelectedIndex == 0 || row.Status == status);
    }
    private void OnFilterChanged(object sender, RoutedEventArgs e)
    {
        if (_view is null) return;
        _view.Refresh();
        UpdateCount();
    }
    private void OnReset(object sender, RoutedEventArgs e) { Filter.Clear(); StatusFilter.SelectedIndex = 0; }
    private void OnTreeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not Node node) return;
        TreeSelection.Text = $"节点：{node.Title}";
        if (node.Title is "在线设备" or "离线设备" or "全部设备")
            StatusFilter.SelectedIndex = node.Title == "在线设备" ? 1 : node.Title == "离线设备" ? 2 : 0;
        Detail.Text = $"{node.Title}\n{(node.OI.Length > 0 ? $"{node.OI} 台设备" : "Gallery 示例资料节点")}";
    }
    private void OnGridSelected(object sender, SelectionChangedEventArgs e)
    {
        if (Detail is null) return;
        Detail.Text = Grid.SelectedItem is Row row ? $"{row.Name}\n{row.Status} · 负载 {row.Load:F1}%" : "当前未选择设备";
        if (_view is not null) UpdateCount();
    }
    private void UpdateCount()
    {
        var count = _view.Cast<Row>().Count();
        Count.Text = $"显示 {count} / 200 台设备 · 已选择 {Grid.SelectedItems.Count} 行";
        Empty.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    // These fields belong to the sample; SUI's tree style has no data-model dependency.
    public sealed class Node(string title, string oi, params Node[] children)
    {
        public string Title { get; } = title;
        public string OI { get; } = oi;
        public ObservableCollection<Node> Children { get; } = new(children);
    }
    public sealed class Row(int id, string name, string status, double load)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public string Status { get; } = status;
        public double Load { get; } = load;
    }
}
