using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SUI;

/// <summary>支持范围约束、小数位数控制、键盘/滚轮步进与步进按钮的数值输入框。</summary>
[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
[TemplatePart(Name = UpButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = DownButtonPartName, Type = typeof(ButtonBase))]
public class SUINumberBox : Control
{
    private const string TextBoxPartName = "PART_TextBox";
    private const string UpButtonPartName = "PART_UpButton";
    private const string DownButtonPartName = "PART_DownButton";

    private TextBox? _textBox;
    private ButtonBase? _upButton;
    private ButtonBase? _downButton;
    private bool _isSyncingText;

    static SUINumberBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUINumberBox), new FrameworkPropertyMetadata(typeof(SUINumberBox)));
        IsTabStopProperty.OverrideMetadata(typeof(SUINumberBox), new FrameworkPropertyMetadata(false));
        IsEnabledProperty.OverrideMetadata(typeof(SUINumberBox), new FrameworkPropertyMetadata(OnStateChanged));
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double?), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(double.MinValue, OnRangeChanged));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(double.MaxValue, OnRangeChanged));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
        nameof(Step), typeof(double), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(1d), value => value is double s && s > 0 && !double.IsNaN(s) && !double.IsInfinity(s));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
        nameof(DecimalPlaces), typeof(int), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(0, OnFormatChanged), value => value is int d && d is >= 0 and <= 15);

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public static readonly DependencyProperty AllowEmptyProperty = DependencyProperty.Register(
        nameof(AllowEmpty), typeof(bool), typeof(SUINumberBox),
        new FrameworkPropertyMetadata(false, OnRangeChanged));

    public bool AllowEmpty
    {
        get => (bool)GetValue(AllowEmptyProperty);
        set => SetValue(AllowEmptyProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(SUINumberBox), new FrameworkPropertyMetadata(false, OnStateChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register(
        nameof(ShowButtons), typeof(bool), typeof(SUINumberBox), new FrameworkPropertyMetadata(true));

    public bool ShowButtons
    {
        get => (bool)GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(SUINumberBox), new FrameworkPropertyMetadata(null));

    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(ValueChanged), RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<double?>), typeof(SUINumberBox));

    public event RoutedPropertyChangedEventHandler<double?> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateEvents();
        base.OnApplyTemplate();

        _textBox = GetTemplateChild(TextBoxPartName) as TextBox;
        _upButton = GetTemplateChild(UpButtonPartName) as ButtonBase;
        _downButton = GetTemplateChild(DownButtonPartName) as ButtonBase;

        if (_textBox is not null)
        {
            _textBox.LostKeyboardFocus += OnTextBoxLostFocus;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }

        if (_upButton is not null) _upButton.Click += OnUpClick;
        if (_downButton is not null) _downButton.Click += OnDownClick;

        SyncTextAndButtons();
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);
        if (IsReadOnly || !IsKeyboardFocusWithin || e.Delta == 0) return;

        StepBy(e.Delta > 0 ? Step : -Step);
        e.Handled = true;
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        if (ReferenceEquals(e.OriginalSource, this) && _textBox is not null)
        {
            _textBox.Focus();
            _textBox.SelectAll();
        }
    }

    private void DetachTemplateEvents()
    {
        if (_textBox is not null)
        {
            _textBox.LostKeyboardFocus -= OnTextBoxLostFocus;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
        }
        if (_upButton is not null) _upButton.Click -= OnUpClick;
        if (_downButton is not null) _downButton.Click -= OnDownClick;
    }

    private void OnUpClick(object sender, RoutedEventArgs e) => StepBy(Step);
    private void OnDownClick(object sender, RoutedEventArgs e) => StepBy(-Step);
    private void OnTextBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e) => CommitInput();

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
                if (!IsReadOnly) StepBy(Step);
                e.Handled = true;
                break;
            case Key.Down:
                if (!IsReadOnly) StepBy(-Step);
                e.Handled = true;
                break;
            case Key.Enter:
                CommitInput();
                e.Handled = true;
                break;
            case Key.Escape:
                SyncTextAndButtons();
                e.Handled = true;
                break;
        }
    }

    public void StepBy(double delta)
    {
        if (IsReadOnly) return;
        var baseValue = TryParseText(out var typed) ? typed : (Value ?? Clamp(0d));
        SetCurrentValue(ValueProperty, Normalize(baseValue + delta));
        SyncTextAndButtons();
    }

    private void CommitInput()
    {
        if (_textBox is null || _isSyncingText) return;

        var raw = _textBox.Text.Trim();
        if (raw.Length == 0 && AllowEmpty)
        {
            SetCurrentValue(ValueProperty, null);
        }
        else if (TryParseText(out var parsed))
        {
            SetCurrentValue(ValueProperty, Normalize(parsed));
        }

        SyncTextAndButtons();
    }

    private bool TryParseText(out double result)
    {
        var raw = _textBox?.Text?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            result = default;
            return false;
        }

        const NumberStyles styles = NumberStyles.Float;
        return double.TryParse(raw, styles, CultureInfo.CurrentCulture, out result)
            || double.TryParse(raw, styles, CultureInfo.InvariantCulture, out result);
    }

    private double Clamp(double value)
    {
        var min = Math.Min(Minimum, Maximum);
        var max = Math.Max(Minimum, Maximum);
        if (value < min) return min;
        return value > max ? max : value;
    }

    private double Normalize(double value) =>
        Math.Round(Clamp(value), DecimalPlaces, MidpointRounding.AwayFromZero);

    private void SyncTextAndButtons()
    {
        if (_textBox is not null)
        {
            _isSyncingText = true;
            try
            {
                _textBox.Text = Value is { } v
                    ? v.ToString($"F{DecimalPlaces}", CultureInfo.CurrentCulture)
                    : string.Empty;
            }
            finally
            {
                _isSyncingText = false;
            }
        }

        var min = Math.Min(Minimum, Maximum);
        var max = Math.Max(Minimum, Maximum);
        var canStep = IsEnabled && !IsReadOnly;
        if (_upButton is not null) _upButton.IsEnabled = canStep && (Value is null || Value < max);
        if (_downButton is not null) _downButton.IsEnabled = canStep && (Value is null || Value > min);
    }

    private static object? CoerceValue(DependencyObject d, object? baseValue)
    {
        var box = (SUINumberBox)d;
        if (baseValue is not double v || double.IsNaN(v) || double.IsInfinity(v))
        {
            return box.AllowEmpty ? null : box.Normalize(0d);
        }
        return box.Normalize(v);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (SUINumberBox)d;
        box.SyncTextAndButtons();
        box.RaiseEvent(new RoutedPropertyChangedEventArgs<double?>((double?)e.OldValue, (double?)e.NewValue, ValueChangedEvent));
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(ValueProperty);
        ((SUINumberBox)d).SyncTextAndButtons();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SUINumberBox)d).SyncTextAndButtons();

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(ValueProperty);
        ((SUINumberBox)d).SyncTextAndButtons();
    }
}
