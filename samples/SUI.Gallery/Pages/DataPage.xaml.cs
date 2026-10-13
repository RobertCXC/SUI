using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SUI.Gallery.Pages;

public partial class DataPage : UserControl
{
    public DataPage()
    {
        InitializeComponent();

        Tree.ItemsSource = new[]
        {
            new Node("设备", "", new Node("COM1", "12"), new Node("COM2", "3")),
            new Node("数据库", "", new Node("users.db", ""), new Node("logs.db", "8")),
        };

        Grid.ItemsSource = Enumerable.Range(1, 200)
            .Select(i => new Row(i, $"设备 {i}", i % 3 == 0 ? "离线" : "在线", i * 1.5))
            .ToList();
    }

    // Title / OI / Children 是 SUITreeNodeTemplate 约定的字段。
    public sealed class Node(string title, string oi, params Node[] children)
    {
        public string Title { get; } = title;
        public string OI { get; } = oi;
        public ObservableCollection<Node> Children { get; } = new(children);
    }

    public sealed record Row(int Id, string Name, string Status, double Value);
}
