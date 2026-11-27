using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SUI;

/// <summary>
/// 轻量时间选择控件，支持直接输入与下拉分栏选择（时/分/可选秒）。
/// </summary>
public partial class TimePicker : UserControl
{
    private static readonly string[] TimeFormats = [@"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss"];
    private bool _syncing;

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        IsEnabledProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(OnIsEnabledChanged));
    }

    public TimePicker()
    {
        InitializeComponent();
        PART_TextBox.LostKeyboardFocus += OnTextBoxLostFocus;
        PART_TextBox.KeyDown += OnTextBoxKeyDown;
        PART_TextBox.TextChanged += OnTextBoxTextChanged;
        RebuildColumns();
        SyncTextFromValue();
    }

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
        nameof(SelectedTime), typeof(TimeSpan?), typeof(TimePicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged, CoerceSelectedTime));

    /// <summary>当前选中的一天内时间（00:00:00 至 23:59:59），可为 null。</summary>
    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register(
        nameof(ShowSeconds), typeof(bool), typeof(TimePicker),
        new FrameworkPropertyMetadata(false, OnFormatOrColumnChanged));

    /// <summary>是否显示并选择秒数；默认 false（HH:mm）。</summary>
    public bool ShowSeconds
    {
        get => (bool)GetValue(ShowSecondsProperty);
        set => SetValue(ShowSecondsProperty, value);
    }

    public static readonly DependencyProperty MinuteIncrementProperty = DependencyProperty.Register(
        nameof(MinuteIncrement), typeof(int), typeof(TimePicker),
        new FrameworkPropertyMetadata(1, OnFormatOrColumnChanged, CoerceIncrement));

    /// <summary>下拉分钟列表步长（1–60）。</summary>
    public int MinuteIncrement
    {
        get => (int)GetValue(MinuteIncrementProperty);
        set => SetValue(MinuteIncrementProperty, value);
    }

    public static readonly DependencyProperty SecondIncrementProperty = DependencyProperty.Register(
        nameof(SecondIncrement), typeof(int), typeof(TimePicker),
        new FrameworkPropertyMetadata(1, OnFormatOrColumnChanged, CoerceIncrement));

    /// <summary>下拉秒数列表步长（1–60）。</summary>
    public int SecondIncrement
    {
        get => (int)GetValue(SecondIncrementProperty);
        set => SetValue(SecondIncrementProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(TimePicker),
        new FrameworkPropertyMetadata("选择时间"));

    /// <summary>输入框为空时显示的占位提示。</summary>
    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(TimePicker),
        new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

    /// <summary>只读状态，禁止编辑与展开下拉面板。</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(TimePicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownOpenChanged, CoerceDropDownOpen));

    /// <summary>时间选择下拉面板是否展开。</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectedTimeChanged), RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<TimeSpan?>), typeof(TimePicker));

    /// <summary>选中时间变化时触发。</summary>
    public event RoutedPropertyChangedEventHandler<TimeSpan?> SelectedTimeChanged
    {
        add => AddHandler(SelectedTimeChangedEvent, value);
        remove => RemoveHandler(SelectedTimeChangedEvent, value);
    }

    private static object? CoerceSelectedTime(DependencyObject d, object? baseValue)
    {
        if (baseValue is not TimeSpan ts) return null;
        if (ts < TimeSpan.Zero) return TimeSpan.Zero;
        if (ts >= TimeSpan.FromDays(1)) return new TimeSpan(23, 59, 59);
        return new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);
    }

    private static object CoerceIncrement(DependencyObject d, object baseValue) =>
        (int)baseValue switch { < 1 => 1, > 60 => 60, var v => v };

    private static object CoerceDropDownOpen(DependencyObject d, object baseValue)
    {
        var picker = (TimePicker)d;
        return (bool)baseValue && !picker.IsReadOnly && picker.IsEnabled;
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        if (!(bool)e.NewValue) picker.SetCurrentValue(IsDropDownOpenProperty, false);
        picker.CoerceValue(IsDropDownOpenProperty);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        if ((bool)e.NewValue) picker.SetCurrentValue(IsDropDownOpenProperty, false);
        picker.CoerceValue(IsDropDownOpenProperty);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.SyncTextFromValue();
        picker.SyncListsFromValue();
        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<TimeSpan?>(
            (TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue, SelectedTimeChangedEvent));
    }

    private static void OnFormatOrColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.RebuildColumns();
        picker.SyncTextFromValue();
        picker.SyncListsFromValue();
    }

    private static void OnDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        if ((bool)e.NewValue)
        {
            picker.CommitText();
            picker.SyncListsFromValue();
        }
    }

    private void RebuildColumns()
    {
        _syncing = true;
        try
        {
            HourList.ItemsSource = Enumerable.Range(0, 24).Select(i => i.ToString("D2", CultureInfo.InvariantCulture)).ToList();
            MinuteList.ItemsSource = BuildRange(MinuteIncrement);
            SecondList.ItemsSource = BuildRange(SecondIncrement);
            SecondColumn.Visibility = ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
        }
        finally
        {
            _syncing = false;
        }
    }

    private static List<string> BuildRange(int step)
    {
        var list = new List<string>();
        for (var i = 0; i < 60; i += step)
            list.Add(i.ToString("D2", CultureInfo.InvariantCulture));
        return list;
    }

    private void SyncTextFromValue()
    {
        _syncing = true;
        try
        {
            var value = SelectedTime;
            PART_TextBox.Text = value.HasValue
                ? (ShowSeconds
                    ? $"{value.Value.Hours:D2}:{value.Value.Minutes:D2}:{value.Value.Seconds:D2}"
                    : $"{value.Value.Hours:D2}:{value.Value.Minutes:D2}")
                : string.Empty;
            UpdatePlaceholderVisibility();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void SyncListsFromValue()
    {
        _syncing = true;
        try
        {
            var value = SelectedTime;
            if (!value.HasValue)
            {
                HourList.SelectedIndex = -1;
                MinuteList.SelectedIndex = -1;
                SecondList.SelectedIndex = -1;
                return;
            }

            SelectItem(HourList, value.Value.Hours.ToString("D2", CultureInfo.InvariantCulture));
            SelectItem(MinuteList, value.Value.Minutes.ToString("D2", CultureInfo.InvariantCulture));
            if (ShowSeconds)
                SelectItem(SecondList, value.Value.Seconds.ToString("D2", CultureInfo.InvariantCulture));
        }
        finally
        {
            _syncing = false;
        }
    }

    private static void SelectItem(ListBox list, string text)
    {
        list.SelectedItem = list.Items.Contains(text) ? text : null;
        if (list.SelectedItem != null) list.ScrollIntoView(list.SelectedItem);
    }

    private void UpdatePlaceholderVisibility() =>
        PlaceholderBlock.Visibility = string.IsNullOrEmpty(PART_TextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e) => UpdatePlaceholderVisibility();

    private void OnTextBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e) => CommitText();

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitText();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SyncTextFromValue();
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
    }

    private void CommitText()
    {
        if (_syncing || IsReadOnly) return;

        var raw = PART_TextBox.Text.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            SetCurrentValue(SelectedTimeProperty, null);
            return;
        }

        if (TryParseTime(raw, out var parsed))
        {
            SetCurrentValue(SelectedTimeProperty, ShowSeconds
                ? parsed
                : new TimeSpan(parsed.Hours, parsed.Minutes, 0));
        }

        SyncTextFromValue();
    }

    private static bool TryParseTime(string text, out TimeSpan result)
    {
        if (TimeSpan.TryParseExact(text, TimeFormats, CultureInfo.InvariantCulture, out var parsed) &&
            parsed >= TimeSpan.Zero && parsed < TimeSpan.FromDays(1))
        {
            result = parsed;
            return true;
        }

        result = default;
        return false;
    }

    private void OnTimePartSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || IsReadOnly) return;

        var current = SelectedTime ?? TimeSpan.Zero;
        var hour = ParseSelected(HourList, current.Hours);
        var minute = ParseSelected(MinuteList, current.Minutes);
        var second = ShowSeconds ? ParseSelected(SecondList, current.Seconds) : 0;
        SetCurrentValue(SelectedTimeProperty, new TimeSpan(hour, minute, second));
    }

    private static int ParseSelected(ListBox list, int fallback) =>
        list.SelectedItem is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : fallback;

    private void OnNowClick(object sender, RoutedEventArgs e)
    {
        if (IsReadOnly) return;
        var now = DateTime.Now.TimeOfDay;
        SetCurrentValue(SelectedTimeProperty, ShowSeconds
            ? new TimeSpan(now.Hours, now.Minutes, now.Seconds)
            : new TimeSpan(now.Hours, now.Minutes, 0));
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (IsReadOnly) return;
        SetCurrentValue(SelectedTimeProperty, null);
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e) =>
        SetCurrentValue(IsDropDownOpenProperty, false);
}
