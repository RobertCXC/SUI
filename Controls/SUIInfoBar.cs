using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SUI;

/// <summary>提示信息语义级别。</summary>
public enum InfoSeverity { Info, Success, Warning, Error }

/// <summary>用于页面内状态提醒、校验告警与操作反馈的内嵌横幅控件。</summary>
[TemplatePart(Name = CloseButtonPartName, Type = typeof(ButtonBase))]
public class SUIInfoBar : ContentControl
{
    private const string CloseButtonPartName = "PART_CloseButton";
    private ButtonBase? _closeButton;

    static SUIInfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIInfoBar), new FrameworkPropertyMetadata(typeof(SUIInfoBar)));
        FocusableProperty.OverrideMetadata(typeof(SUIInfoBar), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUIInfoBar), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SUIInfoBar), new FrameworkPropertyMetadata(null));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(SUIInfoBar), new FrameworkPropertyMetadata(null));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
        nameof(ActionContent), typeof(object), typeof(SUIInfoBar), new FrameworkPropertyMetadata(null));

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
        nameof(Severity), typeof(InfoSeverity), typeof(SUIInfoBar),
        new FrameworkPropertyMetadata(InfoSeverity.Info),
        value => Enum.IsDefined(typeof(InfoSeverity), value));

    public InfoSeverity Severity
    {
        get => (InfoSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(SUIInfoBar), new FrameworkPropertyMetadata(true));

    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(SUIInfoBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SUIInfoBar));

    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClick;
        }

        base.OnApplyTemplate();
        _closeButton = GetTemplateChild(CloseButtonPartName) as ButtonBase;
        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClick;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsOpenProperty, false);
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new InfoBarAutomationPeer(this);

    private sealed class InfoBarAutomationPeer(SUIInfoBar owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(SUIInfoBar);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.StatusBar;
        protected override string GetNameCore()
        {
            if (base.GetNameCore() is { Length: > 0 } name) return name;
            return Owner is SUIInfoBar bar ? $"{bar.Title} {bar.Message}".Trim() : string.Empty;
        }
    }
}
