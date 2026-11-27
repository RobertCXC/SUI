using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SUI;

/// <summary>对话框操作结果。</summary>
public enum SUIDialogResult { None, Primary, Secondary, Cancel }

/// <summary>采用 SUI Fluent 外观的轻量模态确认与提示对话框。</summary>
public class SUIDialog : Window
{
    public SUIDialogResult Result { get; private set; } = SUIDialogResult.None;

    private SUIDialog(
        object content,
        string title,
        string? primaryText,
        string? secondaryText,
        string? cancelText,
        InfoSeverity severity,
        bool isDanger,
        Window? owner)
    {
        if (TryFindResource("SUI.Button.Primary") is null)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/SUI;component/SUIStyles.xaml", UriKind.Relative),
            });
        }

        Title = title;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 360;
        MaxWidth = 480;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        var resolvedOwner = owner ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w != this);
        if (resolvedOwner is not null)
        {
            Owner = resolvedOwner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        Content = BuildLayout(content, title, primaryText, secondaryText, cancelText, severity, isDanger);
    }

    private UIElement BuildLayout(
        object content,
        string title,
        string? primaryText,
        string? secondaryText,
        string? cancelText,
        InfoSeverity severity,
        bool isDanger)
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
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBar = new Grid { Background = Brushes.Transparent, Margin = new Thickness(20, 14, 14, 8) };
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        titleBar.MouseLeftButtonDown += (_, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

        var (glyph, brushKey) = severity switch
        {
            InfoSeverity.Success => ("\uE73E", "SUI.Brush.Success"),
            InfoSeverity.Warning => ("\uE7BA", "SUI.Brush.Warning"),
            InfoSeverity.Error => ("\uEA39", "SUI.Brush.Danger"),
            _ => ("\uE946", isDanger ? "SUI.Brush.Danger" : "SUI.Brush.Info"),
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
            Text = title,
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
        closeBtn.Click += (_, _) => CloseWith(SUIDialogResult.Cancel, false);
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

        var bodyHost = new Border { Margin = new Thickness(20, 4, 20, 20), Child = bodyElement };
        Grid.SetRow(bodyHost, 1);

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

        void AddActionButton(string? text, SUIDialogResult result, bool dialogBool, string? styleKey = null, bool isDefault = false, bool isCancel = false)
        {
            if (string.IsNullOrEmpty(text)) return;
            var btn = new Button
            {
                Content = text,
                MinWidth = 76,
                IsDefault = isDefault,
                IsCancel = isCancel,
                Margin = new Thickness(buttonPanel.Children.Count > 0 ? 8 : 0, 0, 0, 0),
            };
            if (styleKey is not null) btn.Style = (Style)FindResource(styleKey);
            btn.Click += (_, _) => CloseWith(result, dialogBool);
            buttonPanel.Children.Add(btn);
        }

        AddActionButton(primaryText, SUIDialogResult.Primary, true, isDanger ? "SUI.Button.Danger" : "SUI.Button.Primary", isDefault: !isDanger);
        AddActionButton(secondaryText, SUIDialogResult.Secondary, false);
        AddActionButton(cancelText, SUIDialogResult.Cancel, false, isCancel: true);

        footerBorder.Child = buttonPanel;
        Grid.SetRow(footerBorder, 2);

        rootGrid.Children.Add(titleBar);
        rootGrid.Children.Add(bodyHost);
        rootGrid.Children.Add(footerBorder);
        rootBorder.Child = rootGrid;
        return rootBorder;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            CloseWith(SUIDialogResult.Cancel, false);
            e.Handled = true;
        }
    }

    private void CloseWith(SUIDialogResult result, bool? dialogResult)
    {
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
}
