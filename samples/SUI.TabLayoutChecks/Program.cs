using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

internal static class Program
{
    private static readonly List<object> Results = [];
    [STAThread]
    private static int Main(string[] args)
    {
        Application app;
        Assembly? host = null;
        if (args.Length > 0)
        {
            host = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            app = (Application)Activator.CreateInstance(host.GetType("SerialLink.App.App")!)!;
            app.GetType().GetMethod("InitializeComponent")!.Invoke(app, null);
        }
        else
        {
            app = new Application();
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/SUI;component/SUIStyles.xaml", UriKind.Relative) });
        }
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            MinimalLayout();
            if (host != null) CheckHost(host);
            return 0;
        }
        catch (Exception ex) { Results.Add(new { failure = ex.ToString() }); return 1; }
        finally
        {
            var output = Path.GetFullPath("artifacts/tab-layout-results.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            var json = JsonSerializer.Serialize(new { assembly = typeof(SUI.SUICard).Assembly.FullName, results = Results }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(output, json);
            Console.WriteLine(json);
            app.Shutdown();
        }
    }
    private static void Check(string name, bool pass, object? detail = null)
    {
        Results.Add(new { name, pass, detail });
        if (!pass) throw new InvalidOperationException(name);
    }
    private static void Layout(Window window)
    {
        window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        window.UpdateLayout();
    }
    private static ContentPresenter Host(TabControl tabs) => (ContentPresenter)tabs.Template.FindName("PART_SelectedContentHost", tabs);
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }
    private static void MinimalLayout()
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var table = new DataGrid { ItemsSource = new[] { new { Name = "A" }, new { Name = "B" } }, AutoGenerateColumns = true };
        var button = new Button { Content = "发送所选", Margin = new Thickness(0, 12, 0, 0) };
        Grid.SetRow(button, 1); grid.Children.Add(table); grid.Children.Add(button);
        var plain = new Grid { Background = Brushes.AliceBlue };
        var first = new TabItem { Header = new StackPanel { Orientation = Orientation.Horizontal, Children = { new TextBlock { Text = "▤" }, new AccessText { Text = "多条发送(_M)" } } }, Content = grid };
        var tabs = new TabControl { Items = { first, new TabItem { Header = "Grid", Content = plain }, new TabItem { Header = "Text", Content = "简单文本" }, new TabItem { Header = "Center", HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Content = new TextBlock { Text = "居中" } }, new TabItem { Header = "Disabled", IsEnabled = false } } };
        var window = new Window { Title = "SUI NuGet Tab layout checks", Width = 900, Height = 760, Content = tabs };
        window.Show(); Layout(window);
        foreach (var size in new[] { (900d, 760d), (650d, 500d), (1100d, 900d) })
        {
            window.Width = size.Item1; window.Height = size.Item2;
            foreach (int index in new[] { 0, 1, 2, 3, 0 })
            {
                tabs.SelectedIndex = index; Layout(window);
                var presenter = Host(tabs);
                bool center = index == 3;
                Check($"runtime alignment {size} tab {index}", presenter.HorizontalAlignment == (center ? HorizontalAlignment.Center : HorizontalAlignment.Stretch) && presenter.VerticalAlignment == (center ? VerticalAlignment.Center : VerticalAlignment.Stretch), new { presenter.ActualWidth, presenter.ActualHeight });
                var area = (FrameworkElement)VisualTreeHelper.GetParent(presenter);
                if (center)
                {
                    var point = presenter.TranslatePoint(new Point(), area);
                    Check("explicit Center geometry", Math.Abs(point.X * 2 + presenter.ActualWidth - area.ActualWidth) < 2 && presenter.ActualHeight < 60);
                }
                if (index == 0)
                {
                    Check("star row fills and footer at bottom", Math.Abs(grid.ActualHeight + 24 - presenter.ActualHeight) < 2 && Math.Abs(table.ActualHeight + button.ActualHeight + 12 - grid.ActualHeight) < 2 && table.ActualHeight > 250, new { table.ActualHeight, gridHeight = grid.ActualHeight, footerY = button.TranslatePoint(new Point(), grid).Y });
                }
                if (index == 1) Check("plain Grid fills", Math.Abs(plain.ActualHeight - presenter.ActualHeight) < 2);
            }
        }
        var header = Descendants<ContentPresenter>(first).Single(p => p.Content == first.Header);
        Check("header independent Center", header.HorizontalAlignment == HorizontalAlignment.Center && header.VerticalAlignment == VerticalAlignment.Center);
        window.Activate(); Keyboard.Focus(first); Layout(window);
        Check("keyboard focus outline", first.IsKeyboardFocused && ((SolidColorBrush)((Border)first.Template.FindName("FocusOutline", first)).BorderBrush).Color == ((SolidColorBrush)first.FindResource("SUI.Brush.Focus")).Color, new { first.IsKeyboardFocused, brush = ((Border)first.Template.FindName("FocusOutline", first)).BorderBrush.ToString() });
        var end = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(tabs), 0, Key.End) { RoutedEvent = Keyboard.KeyDownEvent };
        tabs.RaiseEvent(end); Layout(window);
        Check("keyboard End skips disabled", tabs.SelectedIndex == 3);
        var home = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(tabs), 0, Key.Home) { RoutedEvent = Keyboard.KeyDownEvent };
        tabs.RaiseEvent(home); Layout(window);
        Check("keyboard Home returns to first", tabs.SelectedIndex == 0);
        Check("selection indicator visible", ((Border)first.Template.FindName("SelectedIndicator", first)).Visibility == Visibility.Visible);
        window.Close();
    }
    private static void CheckHost(Assembly assembly)
    {
        var window = (Window)Activator.CreateInstance(assembly.GetType("SerialLink.App.MainWindow")!)!;
        window.Show(); Layout(window);
        var tabs = (TabControl)window.FindName("CommandTabs");
        var table = (DataGrid)window.FindName("CommandGrid");
        foreach (TabItem tab in tabs.Items)
            Check("SerialLink has no local alignment workaround", tab.ReadLocalValue(Control.HorizontalContentAlignmentProperty) == DependencyProperty.UnsetValue && tab.ReadLocalValue(Control.VerticalContentAlignmentProperty) == DependencyProperty.UnsetValue);
        foreach (var size in new[] { (1500d, 900d), (1150d, 700d), (1500d, 900d) })
        {
            window.Width = size.Item1; window.Height = size.Item2;
            foreach (int i in new[] { 0, 1, 2, 0 })
            {
                tabs.SelectedIndex = i; Layout(window);
                Check($"SerialLink runtime Stretch {size} tab {i}", Host(tabs).VerticalAlignment == VerticalAlignment.Stretch && Host(tabs).HorizontalAlignment == HorizontalAlignment.Stretch);
                if (i == 0) Check("SerialLink DataGrid fills", table.ActualHeight > 250, new { table.ActualHeight, hostHeight = Host(tabs).ActualHeight });
            }
        }
        window.Close();
    }
}
