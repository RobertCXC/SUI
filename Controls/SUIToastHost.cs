using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SUI;

/// <summary>单条轻提示数据项。</summary>
public sealed class SUIToastMessage(string message, string? title = null, InfoSeverity severity = InfoSeverity.Info)
{
    public string Message { get; } = message;
    public string? Title { get; } = title;
    public InfoSeverity Severity { get; } = severity;
}

/// <summary>轻量非阻塞提示宿主容器，可叠加放置在窗口或页面顶层用于弹出短时 Toast。</summary>
public class SUIToastHost : ItemsControl
{
    private readonly ObservableCollection<SUIToastMessage> _toasts = [];

    public static readonly RoutedCommand DismissCommand = new(nameof(DismissCommand), typeof(SUIToastHost));

    static SUIToastHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIToastHost), new FrameworkPropertyMetadata(typeof(SUIToastHost)));
        FocusableProperty.OverrideMetadata(typeof(SUIToastHost), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUIToastHost), new FrameworkPropertyMetadata(false));
        CommandManager.RegisterClassCommandBinding(typeof(SUIToastHost),
            new CommandBinding(DismissCommand, (s, e) => (s as SUIToastHost)?.Dismiss(e.Parameter as SUIToastMessage)));
    }

    public SUIToastHost() => ItemsSource = _toasts;

    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration), typeof(TimeSpan), typeof(SUIToastHost), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(3)));

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty MaxItemsProperty = DependencyProperty.Register(
        nameof(MaxItems), typeof(int), typeof(SUIToastHost),
        new FrameworkPropertyMetadata(4), value => value is int n && n > 0);

    public int MaxItems
    {
        get => (int)GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    public SUIToastMessage Show(string message, InfoSeverity severity = InfoSeverity.Info, string? title = null, TimeSpan? duration = null)
    {
        while (_toasts.Count >= MaxItems)
        {
            _toasts.RemoveAt(0);
        }

        var item = new SUIToastMessage(message, title, severity);
        _toasts.Add(item);

        var delay = duration ?? Duration;
        if (delay > TimeSpan.Zero)
        {
            var timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher) { Interval = delay };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _toasts.Remove(item);
            };
            timer.Start();
        }

        return item;
    }

    public void Dismiss(SUIToastMessage? item)
    {
        if (item is not null)
        {
            _toasts.Remove(item);
        }
    }

    public void Clear() => _toasts.Clear();
}
