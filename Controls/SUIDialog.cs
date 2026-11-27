using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SUI;

/// <summary>对话框操作结果。</summary>
public enum SUIDialogResult { None, Primary, Secondary, Cancel }

/// <summary>对话框关闭前校验与拦截事件参数。</summary>
public sealed class SUIDialogClosingEventArgs(SUIDialogResult result) : CancelEventArgs
{
    /// <summary>即将执行的关闭结果。</summary>
    public SUIDialogResult Result { get; } = result;

    /// <summary>拦截关闭（Cancel=true）时在对话框底部原地展示的错误提示文本。</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>模态对话框尺寸、滚动与校验配置。</summary>
public sealed class SUIDialogOptions
{
    public string Title { get; set; } = "提示";
    public string? PrimaryText { get; set; } = "确定";
    public string? SecondaryText { get; set; }
    public string? CancelText { get; set; }
    public InfoSeverity Severity { get; set; } = InfoSeverity.Info;
    public bool IsDanger { get; set; }
    public Window? Owner { get; set; }

    public double Width { get; set; } = double.NaN;
    public double MinWidth { get; set; } = 360;
    public double MaxWidth { get; set; } = 480;
    public double Height { get; set; } = double.NaN;
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; } = double.NaN;

    public bool UseScrollViewer { get; set; } = true;
    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Disabled;
    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

    /// <summary>关闭前回调（可拦截任意关闭并设置 ErrorMessage 原地提示）。</summary>
    public EventHandler<SUIDialogClosingEventArgs>? Closing { get; set; }

    /// <summary>可选绑定到主确认按钮的命令（自动联动 CanExecute 禁用态，并在校验通过确认关闭前执行）。</summary>
    public ICommand? PrimaryCommand { get; set; }
    public object? PrimaryCommandParameter { get; set; }

    /// <summary>可选绑定到主确认按钮 IsEnabled 属性的启用条件绑定。</summary>
    public BindingBase? PrimaryEnabledBinding { get; set; }
}

/// <summary>采用 SUI Fluent 外观的轻量模态确认、提示与表单对话框。</summary>
public class SUIDialog : Window
{
    private readonly ICommand? _primaryCommand;
    private readonly object? _primaryCommandParameter;
    private readonly bool _hasPrimaryEnabledBinding;
    private Button? _primaryButton;
    private SUIInfoBar? _errorBar;

    public SUIDialogResult Result { get; private set; } = SUIDialogResult.None;

    /// <summary>当前在对话框底部原地展示的校验错误信息。</summary>
    public string? ValidationError
    {
        get => _errorBar?.Message;
        set => SetValidationError(value);
    }

    /// <summary>关闭前触发，可用于执行表单校验或阻止关闭。</summary>
    public event EventHandler<SUIDialogClosingEventArgs>? DialogClosing;

    public SUIDialog(object content, SUIDialogOptions? options = null)
    {
        options ??= new SUIDialogOptions();
        _primaryCommand = options.PrimaryCommand;
        _primaryCommandParameter = options.PrimaryCommandParameter;
        _hasPrimaryEnabledBinding = options.PrimaryEnabledBinding is not null;
        if (options.Closing is not null)
        {
            DialogClosing += options.Closing;
        }

        if (TryFindResource("SUI.Button.Primary") is null)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/SUI;component/SUIStyles.xaml", UriKind.Relative),
            });
        }

        Title = options.Title;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        MinWidth = options.MinWidth;
        MaxWidth = options.MaxWidth;
        MinHeight = options.MinHeight;
        if (!double.IsNaN(options.Width)) Width = options.Width;
        if (!double.IsNaN(options.Height)) Height = options.Height;

        MaxHeight = !double.IsNaN(options.MaxHeight)
            ? options.MaxHeight
            : Math.Max(320d, SystemParameters.WorkArea.Height * 0.85);

        SizeToContent = (!double.IsNaN(options.Width), !double.IsNaN(options.Height)) switch
        {
            (true, true) => SizeToContent.Manual,
            (true, false) => SizeToContent.Height,
            (false, true) => SizeToContent.Width,
            _ => SizeToContent.WidthAndHeight,
        };

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        var resolvedOwner = options.Owner ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w.IsVisible && w != this);
        if (resolvedOwner is { IsVisible: true })
        {
            Owner = resolvedOwner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        Content = BuildLayout(content, options);
    }

    private SUIDialog(
        object content,
        string title,
        string? primaryText,
        string? secondaryText,
        string? cancelText,
        InfoSeverity severity,
        bool isDanger,
        Window? owner)
        : this(content, new SUIDialogOptions
        {
            Title = title,
            PrimaryText = primaryText,
            SecondaryText = secondaryText,
            CancelText = cancelText,
            Severity = severity,
            IsDanger = isDanger,
            Owner = owner,
        })
    {
    }

    /// <summary>设置或清空对话框底部的原地错误提示横幅。</summary>
    public void SetValidationError(string? message)
    {
        if (_errorBar is null) return;

        if (string.IsNullOrWhiteSpace(message))
        {
            _errorBar.Message = null;
            _errorBar.IsOpen = false;
            _errorBar.Visibility = Visibility.Collapsed;
        }
        else
        {
            _errorBar.Message = message;
            _errorBar.IsOpen = true;
            _errorBar.Visibility = Visibility.Visible;
        }
    }

    private UIElement BuildLayout(object content, SUIDialogOptions options)
    {
        var panelRadius = (CornerRadius)FindResource("SUI.Radius.Panel");
        var borderBrush = (Brush)FindResource("SUI.Brush.Border");
        var rootBorder = new Border
        {
            Background = (Brush)FindResource("SUI.Brush.Surface"),
            BorderBrush = borderBrush,
            BorderThickness = (Thickness)FindResource("SUI.Border.Thickness"),
            CornerRadius = panelRadius,
            Margin = new Thickness(12),
            SnapsToDevicePixels = true,
            Effect = new DropShadowEffect
            {
                BlurRadius = 12,
                ShadowDepth = 3,
                Opacity = 0.18,
                Color = borderBrush is SolidColorBrush scb ? scb.Color : Colors.Black,
            },
        };

        var rootGrid = new Grid();
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBar = new Grid { Background = Brushes.Transparent, Margin = new Thickness(20, 14, 14, 8) };
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleBar.MouseLeftButtonDown += (_, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

        var (glyph, brushKey) = options.Severity switch
        {
            InfoSeverity.Success => ("\uE73E", "SUI.Brush.Success"),
            InfoSeverity.Warning => ("\uE7BA", "SUI.Brush.Warning"),
            InfoSeverity.Error => ("\uEA39", "SUI.Brush.Danger"),
            _ => ("\uE946", options.IsDanger ? "SUI.Brush.Danger" : "SUI.Brush.Info"),
        };

        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = (FontFamily)FindResource("SUI.Font.Icon"),
            FontSize = 16,
            Foreground = (Brush)FindResource(brushKey),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };
        Grid.SetColumn(icon, 0);

        var titleText = new TextBlock
        {
            Text = options.Title,
            FontFamily = (FontFamily)FindResource("SUI.Font.Family"),
            FontSize = (double)FindResource("SUI.Font.Size.Title"),
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("SUI.Brush.Text"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetColumn(titleText, 1);

        var closeBtn = new Button
        {
            Content = "\uE711",
            FontFamily = (FontFamily)FindResource("SUI.Font.Icon"),
            FontSize = 11,
            Width = 28,
            Height = 28,
            MinHeight = 28,
            Padding = new Thickness(0),
            Style = (Style)FindResource("SUI.Button.Ghost"),
        };
        AutomationProperties.SetName(closeBtn, "关闭对话框");
        closeBtn.Click += (_, _) => TryCloseWith(SUIDialogResult.Cancel, false);
        Grid.SetColumn(closeBtn, 2);

        titleBar.Children.Add(icon);
        titleBar.Children.Add(titleText);
        titleBar.Children.Add(closeBtn);
        Grid.SetRow(titleBar, 0);

        UIElement bodyElement = content is UIElement ui
            ? ui
            : new TextBlock
            {
                Text = content?.ToString() ?? string.Empty,
                FontFamily = (FontFamily)FindResource("SUI.Font.Family"),
                FontSize = (double)FindResource("SUI.Font.Size.Body"),
                Foreground = (Brush)FindResource("SUI.Brush.TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
            };

        var bodyHost = new Border { Padding = new Thickness(20, 4, 20, 16), Child = bodyElement };
        UIElement bodyContainer = options.UseScrollViewer
            ? new ScrollViewer
            {
                HorizontalScrollBarVisibility = options.HorizontalScrollBarVisibility,
                VerticalScrollBarVisibility = options.VerticalScrollBarVisibility,
                Content = bodyHost,
            }
            : bodyHost;
        Grid.SetRow(bodyContainer, 1);

        _errorBar = new SUIInfoBar
        {
            Severity = InfoSeverity.Error,
            IsClosable = false,
            IsOpen = false,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(20, 0, 20, 12),
        };
        Grid.SetRow(_errorBar, 2);

        var footerBorder = new Border
        {
            Background = (Brush)FindResource("SUI.Brush.SurfaceAlt"),
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            CornerRadius = new CornerRadius(0, 0, panelRadius.BottomRight, panelRadius.BottomLeft),
            Padding = new Thickness(20, 12, 20, 12),
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        Button? AddActionButton(string? text, SUIDialogResult result, bool dialogBool, string? styleKey = null, bool isDefault = false, bool isCancel = false)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var btn = new Button
            {
                Content = text,
                MinWidth = 76,
                IsDefault = isDefault,
                IsCancel = isCancel,
                Margin = new Thickness(buttonPanel.Children.Count > 0 ? 8 : 0, 0, 0, 0),
            };
            if (styleKey is not null) btn.Style = (Style)FindResource(styleKey);
            btn.Click += (_, _) => TryCloseWith(result, dialogBool);
            buttonPanel.Children.Add(btn);
            return btn;
        }

        _primaryButton = AddActionButton(
            options.PrimaryText,
            SUIDialogResult.Primary,
            true,
            options.IsDanger ? "SUI.Button.Danger" : "SUI.Button.Primary",
            isDefault: !options.IsDanger);

        if (_primaryButton is not null)
        {
            if (options.PrimaryEnabledBinding is not null)
            {
                _primaryButton.SetBinding(IsEnabledProperty, options.PrimaryEnabledBinding);
            }
            if (_primaryCommand is not null)
            {
                if (!_hasPrimaryEnabledBinding)
                {
                    _primaryButton.IsEnabled = _primaryCommand.CanExecute(_primaryCommandParameter);
                }
                _primaryCommand.CanExecuteChanged += OnPrimaryCommandCanExecuteChanged;
            }
        }

        AddActionButton(options.SecondaryText, SUIDialogResult.Secondary, false);
        AddActionButton(options.CancelText, SUIDialogResult.Cancel, false, isCancel: true);

        footerBorder.Child = buttonPanel;
        Grid.SetRow(footerBorder, 3);

        rootGrid.Children.Add(titleBar);
        rootGrid.Children.Add(bodyContainer);
        rootGrid.Children.Add(_errorBar);
        rootGrid.Children.Add(footerBorder);
        rootBorder.Child = rootGrid;
        return rootBorder;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            TryCloseWith(SUIDialogResult.Cancel, false);
            e.Handled = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_primaryCommand is not null)
        {
            _primaryCommand.CanExecuteChanged -= OnPrimaryCommandCanExecuteChanged;
        }
        base.OnClosed(e);
    }

    private void OnPrimaryCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        if (_primaryButton is not null && _primaryCommand is not null && !_hasPrimaryEnabledBinding)
        {
            _primaryButton.IsEnabled = _primaryCommand.CanExecute(_primaryCommandParameter);
        }
    }

    private void TryCloseWith(SUIDialogResult result, bool? dialogResult)
    {
        if (result == SUIDialogResult.Primary)
        {
            SetValidationError(null);
            if (_primaryCommand is not null && !_primaryCommand.CanExecute(_primaryCommandParameter))
            {
                return;
            }
        }

        if (DialogClosing is not null)
        {
            var args = new SUIDialogClosingEventArgs(result);
            DialogClosing.Invoke(this, args);
            if (args.Cancel)
            {
                if (!string.IsNullOrWhiteSpace(args.ErrorMessage))
                {
                    SetValidationError(args.ErrorMessage);
                }
                return;
            }
        }

        if (result == SUIDialogResult.Primary && _primaryCommand is not null)
        {
            _primaryCommand.Execute(_primaryCommandParameter);
        }

        Result = result;
        try
        {
            DialogResult = dialogResult;
        }
        catch (InvalidOperationException)
        {
            Close();
        }
    }

    /// <summary>显示双按钮确认弹窗，点击主确认按钮返回 true，其余返回 false。</summary>
    public static bool Confirm(
        string message,
        string title = "确认操作",
        string confirmText = "确定",
        string cancelText = "取消",
        bool isDanger = false,
        Window? owner = null) =>
        Show(message, title, confirmText, null, cancelText,
            isDanger ? InfoSeverity.Warning : InfoSeverity.Info, isDanger, owner) == SUIDialogResult.Primary;

    /// <summary>显示带可自定义按钮与内容的 SUI 风格模态对话框。</summary>
    public static SUIDialogResult Show(
        object content,
        string title = "提示",
        string? primaryText = "确定",
        string? secondaryText = null,
        string? cancelText = null,
        InfoSeverity severity = InfoSeverity.Info,
        bool isDanger = false,
        Window? owner = null)
    {
        var dialog = new SUIDialog(content, title, primaryText, secondaryText, cancelText, severity, isDanger, owner);
        dialog.ShowDialog();
        return dialog.Result;
    }

    /// <summary>按完整选项（含自定义尺寸上限、滚动区与关闭前校验拦截）显示模态对话框。</summary>
    public static SUIDialogResult Show(object content, SUIDialogOptions options)
    {
        var dialog = new SUIDialog(content, options);
        dialog.ShowDialog();
        return dialog.Result;
    }
}