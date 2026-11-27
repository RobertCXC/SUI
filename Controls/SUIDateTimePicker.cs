using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SUI;

/// <summary>一体化日期与时间选择控件，支持直接键入 yyyy-MM-dd HH:mm:ss 与日历+时分秒联合弹出面板。</summary>
[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
[TemplatePart(Name = PopupPartName, Type = typeof(Popup))]
[TemplatePart(Name = CalendarPartName, Type = typeof(System.Windows.Controls.Calendar))]
[TemplatePart(Name = HourListPartName, Type = typeof(ListBox))]
[TemplatePart(Name = MinuteListPartName, Type = typeof(ListBox))]
[TemplatePart(Name = SecondListPartName, Type = typeof(ListBox))]
[TemplatePart(Name = NowButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = TodayButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = ClearButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = ConfirmButtonPartName, Type = typeof(ButtonBase))]
public class SUIDateTimePicker : Control
{
    private const string TextBoxPartName = "PART_TextBox";
    private const string PopupPartName = "PART_Popup";
    private const string CalendarPartName = "PART_Calendar";
    private const string HourListPartName = "PART_HourList";
    private const string MinuteListPartName = "PART_MinuteList";
    private const string SecondListPartName = "PART_SecondList";
    private const string NowButtonPartName = "PART_NowButton";
    private const string TodayButtonPartName = "PART_TodayButton";
    private const string ClearButtonPartName = "PART_ClearButton";
    private const string ConfirmButtonPartName = "PART_ConfirmButton";

    private static readonly string[] DateTimeFormats =
    [
        "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd H:mm:ss", "yyyy-M-d HH:mm:ss", "yyyy-M-d H:mm:ss",
        "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-M-d HH:mm", "yyyy-M-d H:mm",
        "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd H:mm:ss", "yyyy/M/d HH:mm:ss", "yyyy/M/d H:mm:ss",
        "yyyy/MM/dd HH:mm", "yyyy/MM/dd H:mm", "yyyy/M/d HH:mm", "yyyy/M/d H:mm",
    ];

    private static readonly string[] DateOnlyFormats =
    [
        "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd", "yyyy/M/d",
    ];

    private TextBox? _textBox;
    private Popup? _popup;
    private System.Windows.Controls.Calendar? _calendar;
    private ListBox? _hourList;
    private ListBox? _minuteList;
    private ListBox? _secondList;
    private ButtonBase? _nowButton;
    private ButtonBase? _todayButton;
    private ButtonBase? _clearButton;
    private ButtonBase? _confirmButton;
    private bool _syncing;

    static SUIDateTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIDateTimePicker), new FrameworkPropertyMetadata(typeof(SUIDateTimePicker)));
        IsEnabledProperty.OverrideMetadata(typeof(SUIDateTimePicker), new FrameworkPropertyMetadata(OnIsEnabledChanged));
    }

    public static readonly DependencyProperty SelectedDateTimeProperty = DependencyProperty.Register(
        nameof(SelectedDateTime), typeof(DateTime?), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged, CoerceSelectedDateTime));

    /// <summary>当前选中的日期与时间，可为 null。</summary>
    public DateTime? SelectedDateTime
    {
        get => (DateTime?)GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register(
        nameof(ShowSeconds), typeof(bool), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(true, OnFormatOrColumnChanged));

    /// <summary>是否显示并选择秒数；默认 true（yyyy-MM-dd HH:mm:ss）。</summary>
    public bool ShowSeconds
    {
        get => (bool)GetValue(ShowSecondsProperty);
        set => SetValue(ShowSecondsProperty, value);
    }

    public static readonly DependencyProperty DisplayFormatProperty = DependencyProperty.Register(
        nameof(DisplayFormat), typeof(string), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(null, OnFormatOrColumnChanged));

    /// <summary>可选自定义显示格式；为空时自动按 ShowSeconds 使用 yyyy-MM-dd HH:mm:ss 或 yyyy-MM-dd HH:mm。</summary>
    public string? DisplayFormat
    {
        get => (string?)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    public static readonly DependencyProperty MinimumDateTimeProperty = DependencyProperty.Register(
        nameof(MinimumDateTime), typeof(DateTime?), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(null, OnRangeChanged));

    /// <summary>可选最小允许日期时间。</summary>
    public DateTime? MinimumDateTime
    {
        get => (DateTime?)GetValue(MinimumDateTimeProperty);
        set => SetValue(MinimumDateTimeProperty, value);
    }

    public static readonly DependencyProperty MaximumDateTimeProperty = DependencyProperty.Register(
        nameof(MaximumDateTime), typeof(DateTime?), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(null, OnRangeChanged));

    /// <summary>可选最大允许日期时间。</summary>
    public DateTime? MaximumDateTime
    {
        get => (DateTime?)GetValue(MaximumDateTimeProperty);
        set => SetValue(MaximumDateTimeProperty, value);
    }

    public static readonly DependencyProperty MinuteIncrementProperty = DependencyProperty.Register(
        nameof(MinuteIncrement), typeof(int), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(1, OnFormatOrColumnChanged, CoerceIncrement));

    /// <summary>下拉分钟列表步长（1–60）。</summary>
    public int MinuteIncrement
    {
        get => (int)GetValue(MinuteIncrementProperty);
        set => SetValue(MinuteIncrementProperty, value);
    }

    public static readonly DependencyProperty SecondIncrementProperty = DependencyProperty.Register(
        nameof(SecondIncrement), typeof(int), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(1, OnFormatOrColumnChanged, CoerceIncrement));

    /// <summary>下拉秒数列表步长（1–60）。</summary>
    public int SecondIncrement
    {
        get => (int)GetValue(SecondIncrementProperty);
        set => SetValue(SecondIncrementProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata("选择日期时间"));

    /// <summary>输入框为空时显示的占位提示。</summary>
    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

    /// <summary>只读状态，禁止文本输入与展开下拉面板。</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownOpenChanged, CoerceDropDownOpen));

    /// <summary>联合选择弹出面板是否展开。</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
        nameof(AllowClear), typeof(bool), typeof(SUIDateTimePicker),
        new FrameworkPropertyMetadata(true));

    /// <summary>是否在弹出面板底部显示「清除」按钮。</summary>
    public bool AllowClear
    {
        get => (bool)GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    public static readonly RoutedEvent SelectedDateTimeChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectedDateTimeChanged), RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(SUIDateTimePicker));

    /// <summary>选中日期时间变化时触发。</summary>
    public event RoutedPropertyChangedEventHandler<DateTime?> SelectedDateTimeChanged
    {
        add => AddHandler(SelectedDateTimeChangedEvent, value);
        remove => RemoveHandler(SelectedDateTimeChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateEvents();
        base.OnApplyTemplate();

        _textBox = GetTemplateChild(TextBoxPartName) as TextBox;
        _popup = GetTemplateChild(PopupPartName) as Popup;
        _calendar = GetTemplateChild(CalendarPartName) as System.Windows.Controls.Calendar;
        _hourList = GetTemplateChild(HourListPartName) as ListBox;
        _minuteList = GetTemplateChild(MinuteListPartName) as ListBox;
        _secondList = GetTemplateChild(SecondListPartName) as ListBox;
        _nowButton = GetTemplateChild(NowButtonPartName) as ButtonBase;
        _todayButton = GetTemplateChild(TodayButtonPartName) as ButtonBase;
        _clearButton = GetTemplateChild(ClearButtonPartName) as ButtonBase;
        _confirmButton = GetTemplateChild(ConfirmButtonPartName) as ButtonBase;

        AttachTemplateEvents();
        RebuildTimeColumns();
        SyncCalendarRange();
        SyncTextFromValue();
        SyncPopupFromValue();
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

    private void AttachTemplateEvents()
    {
        if (_textBox is not null)
        {
            _textBox.LostKeyboardFocus += OnTextBoxLostFocus;
            _textBox.KeyDown += OnTextBoxKeyDown;
        }
        if (_calendar is not null)
        {
            _calendar.SelectedDatesChanged += OnCalendarSelectedDatesChanged;
            _calendar.PreviewMouseUp += OnCalendarPreviewMouseUp;
        }
        if (_hourList is not null) _hourList.SelectionChanged += OnTimePartSelectionChanged;
        if (_minuteList is not null) _minuteList.SelectionChanged += OnTimePartSelectionChanged;
        if (_secondList is not null) _secondList.SelectionChanged += OnTimePartSelectionChanged;
        if (_nowButton is not null) _nowButton.Click += OnNowClick;
        if (_todayButton is not null) _todayButton.Click += OnTodayClick;
        if (_clearButton is not null) _clearButton.Click += OnClearClick;
        if (_confirmButton is not null) _confirmButton.Click += OnConfirmClick;
    }

    private void DetachTemplateEvents()
    {
        if (_textBox is not null)
        {
            _textBox.LostKeyboardFocus -= OnTextBoxLostFocus;
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }
        if (_calendar is not null)
        {
            _calendar.SelectedDatesChanged -= OnCalendarSelectedDatesChanged;
            _calendar.PreviewMouseUp -= OnCalendarPreviewMouseUp;
        }
        if (_hourList is not null) _hourList.SelectionChanged -= OnTimePartSelectionChanged;
        if (_minuteList is not null) _minuteList.SelectionChanged -= OnTimePartSelectionChanged;
        if (_secondList is not null) _secondList.SelectionChanged -= OnTimePartSelectionChanged;
        if (_nowButton is not null) _nowButton.Click -= OnNowClick;
        if (_todayButton is not null) _todayButton.Click -= OnTodayClick;
        if (_clearButton is not null) _clearButton.Click -= OnClearClick;
        if (_confirmButton is not null) _confirmButton.Click -= OnConfirmClick;
    }

    private void RebuildTimeColumns()
    {
        _syncing = true;
        try
        {
            if (_hourList is not null) _hourList.ItemsSource = BuildRange(24, 1);
            if (_minuteList is not null) _minuteList.ItemsSource = BuildRange(60, MinuteIncrement);
            if (_secondList is not null) _secondList.ItemsSource = BuildRange(60, SecondIncrement);
        }
        finally
        {
            _syncing = false;
        }
    }

    private static List<string> BuildRange(int exclusiveMax, int step)
    {
        var list = new List<string>(exclusiveMax / Math.Max(1, step) + 1);
        for (var i = 0; i < exclusiveMax; i += Math.Max(1, step))
        {
            list.Add(i.ToString("D2", CultureInfo.InvariantCulture));
        }
        return list;
    }

    private string EffectiveFormat =>
        !string.IsNullOrWhiteSpace(DisplayFormat)
            ? DisplayFormat!
            : (ShowSeconds ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd HH:mm");

    private void SyncTextFromValue()
    {
        if (_textBox is null) return;
        _syncing = true;
        try
        {
            _textBox.Text = SelectedDateTime.HasValue
                ? SelectedDateTime.Value.ToString(EffectiveFormat, CultureInfo.InvariantCulture)
                : string.Empty;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void SyncCalendarRange()
    {
        if (_calendar is null) return;
        _calendar.DisplayDateStart = MinimumDateTime?.Date;
        _calendar.DisplayDateEnd = MaximumDateTime?.Date;
    }

    private void SyncPopupFromValue()
    {
        _syncing = true;
        try
        {
            var value = SelectedDateTime;
            if (_calendar is not null)
            {
                if (value.HasValue)
                {
                    _calendar.SelectedDate = value.Value.Date;
                    _calendar.DisplayDate = value.Value.Date;
                }
                else
                {
                    _calendar.SelectedDates.Clear();
                    _calendar.DisplayDate = DateTime.Today;
                }
            }

            if (value.HasValue)
            {
                SelectListItem(_hourList, value.Value.Hour.ToString("D2", CultureInfo.InvariantCulture));
                SelectListItem(_minuteList, value.Value.Minute.ToString("D2", CultureInfo.InvariantCulture));
                if (ShowSeconds)
                {
                    SelectListItem(_secondList, value.Value.Second.ToString("D2", CultureInfo.InvariantCulture));
                }
            }
            else
            {
                if (_hourList is not null) _hourList.SelectedIndex = -1;
                if (_minuteList is not null) _minuteList.SelectedIndex = -1;
                if (_secondList is not null) _secondList.SelectedIndex = -1;
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    private static void SelectListItem(ListBox? list, string value)
    {
        if (list is null) return;
        list.SelectedItem = list.Items.Contains(value) ? value : null;
        if (list.SelectedItem is not null) list.ScrollIntoView(list.SelectedItem);
    }

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
        if (_syncing || IsReadOnly || _textBox is null) return;

        var raw = _textBox.Text.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            SetCurrentValue(SelectedDateTimeProperty, null);
            return;
        }

        if (TryParseDateTime(raw, out var parsed))
        {
            var normalized = ShowSeconds
                ? new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, parsed.Second)
                : new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, 0);
            SetCurrentValue(SelectedDateTimeProperty, Clamp(normalized));
        }

        SyncTextFromValue();
    }

    private bool TryParseDateTime(string text, out DateTime result)
    {
        if (!string.IsNullOrWhiteSpace(DisplayFormat) &&
            (DateTime.TryParseExact(text, DisplayFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out result) ||
             DateTime.TryParseExact(text, DisplayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)))
        {
            return true;
        }

        if (DateTime.TryParseExact(text, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        if (DateTime.TryParseExact(text, DateOnlyFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            var time = SelectedDateTime?.TimeOfDay ?? TimeSpan.Zero;
            result = dateOnly.Date + (ShowSeconds ? time : new TimeSpan(time.Hours, time.Minutes, 0));
            return true;
        }

        return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out result)
            || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    private static void OnCalendarPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        // Release WPF Calendar's mouse capture so the next click hits time lists or buttons immediately.
        if (Mouse.Captured is CalendarItem)
        {
            Mouse.Capture(null);
        }
    }

    private void OnCalendarSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncing || IsReadOnly || _calendar?.SelectedDate is not DateTime pickedDate) return;

        var existingTime = SelectedDateTime?.TimeOfDay ?? ReadTimeFromLists() ?? TimeSpan.Zero;
        if (!ShowSeconds)
        {
            existingTime = new TimeSpan(existingTime.Hours, existingTime.Minutes, 0);
        }

        SetCurrentValue(SelectedDateTimeProperty, Clamp(pickedDate.Date + existingTime));
    }

    private void OnTimePartSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || IsReadOnly) return;

        var baseDate = SelectedDateTime?.Date ?? _calendar?.SelectedDate?.Date ?? DateTime.Today;
        var current = SelectedDateTime?.TimeOfDay ?? TimeSpan.Zero;
        var hour = ParseSelected(_hourList, current.Hours);
        var minute = ParseSelected(_minuteList, current.Minutes);
        var second = ShowSeconds ? ParseSelected(_secondList, current.Seconds) : 0;

        SetCurrentValue(SelectedDateTimeProperty, Clamp(baseDate + new TimeSpan(hour, minute, second)));
    }

    private TimeSpan? ReadTimeFromLists()
    {
        if (_hourList?.SelectedItem is null && _minuteList?.SelectedItem is null && _secondList?.SelectedItem is null)
            return null;
        return new TimeSpan(
            ParseSelected(_hourList, 0),
            ParseSelected(_minuteList, 0),
            ShowSeconds ? ParseSelected(_secondList, 0) : 0);
    }

    private static int ParseSelected(ListBox? list, int fallback) =>
        list?.SelectedItem is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : fallback;

    private void OnNowClick(object sender, RoutedEventArgs e)
    {
        if (IsReadOnly) return;
        var now = DateTime.Now;
        var value = ShowSeconds
            ? new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
            : new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
        SetCurrentValue(SelectedDateTimeProperty, Clamp(value));
    }

    private void OnTodayClick(object sender, RoutedEventArgs e)
    {
        if (IsReadOnly) return;
        var time = SelectedDateTime?.TimeOfDay ?? TimeSpan.Zero;
        if (!ShowSeconds) time = new TimeSpan(time.Hours, time.Minutes, 0);
        SetCurrentValue(SelectedDateTimeProperty, Clamp(DateTime.Today + time));
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (IsReadOnly) return;
        SetCurrentValue(SelectedDateTimeProperty, null);
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e) =>
        SetCurrentValue(IsDropDownOpenProperty, false);

    private DateTime Clamp(DateTime value)
    {
        if (MinimumDateTime is DateTime min && value < min) value = min;
        if (MaximumDateTime is DateTime max && value > max) value = max;
        return value;
    }

    private static object? CoerceSelectedDateTime(DependencyObject d, object? baseValue)
    {
        if (baseValue is not DateTime dt) return null;
        var picker = (SUIDateTimePicker)d;
        return picker.Clamp(dt);
    }

    private static object CoerceIncrement(DependencyObject d, object baseValue) =>
        baseValue is int n ? Math.Max(1, Math.Min(60, n)) : 1;

    private static object CoerceDropDownOpen(DependencyObject d, object baseValue)
    {
        var picker = (SUIDateTimePicker)d;
        return (bool)baseValue && picker.IsEnabled && !picker.IsReadOnly;
    }

    private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        picker.SyncTextFromValue();
        picker.SyncPopupFromValue();
        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<DateTime?>((DateTime?)e.OldValue, (DateTime?)e.NewValue, SelectedDateTimeChangedEvent));
    }

    private static void OnFormatOrColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        picker.RebuildTimeColumns();
        picker.SyncTextFromValue();
        picker.SyncPopupFromValue();
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        picker.SyncCalendarRange();
        picker.CoerceValue(SelectedDateTimeProperty);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        if ((bool)e.NewValue) picker.SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        if (!(bool)e.NewValue) picker.SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static void OnDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SUIDateTimePicker)d;
        if ((bool)e.NewValue)
        {
            picker.CommitText();
            picker.SyncPopupFromValue();
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new DateTimePickerAutomationPeer(this);

    private sealed class DateTimePickerAutomationPeer(SUIDateTimePicker owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(SUIDateTimePicker);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;
        protected override string GetNameCore()
        {
            if (base.GetNameCore() is { Length: > 0 } name) return name;
            return owner.SelectedDateTime?.ToString(owner.EffectiveFormat, CultureInfo.InvariantCulture)
                ?? owner.PlaceholderText
                ?? nameof(SUIDateTimePicker);
        }
    }
}
