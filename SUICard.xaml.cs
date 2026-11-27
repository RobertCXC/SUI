using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SUI;

/// <summary>
/// 轻量信息与入口卡片控件，支持图标/图片/字形、标题换行、状态徽标插槽、操作插槽及点击/命令交互。
/// </summary>
public partial class SUICard : UserControl, ICommandSource
{
    private static readonly SolidColorBrush DefaultIconBrush = CreateFrozenBrush("#4C79FF");
    private bool _canExecute = true;

    public SUICard()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        UpdateInteractionState();
    }

    private static SolidColorBrush CreateFrozenBrush(string color)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        brush.Freeze();
        return brush;
    }

    /// <summary>图片来源（可为空）；未设置时优先显示 <see cref="IconGlyph"/>，其次显示 <see cref="IconText"/>。</summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(SUICard));

    /// <summary>卡片标题（默认“标题”），支持自动换行。</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SUICard), new PropertyMetadata("标题"));

    /// <summary>卡片描述（默认“略”），为空时自动折叠。</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(SUICard), new PropertyMetadata("略"));

    /// <summary>默认单字或短文字图标（默认“S”）。</summary>
    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }
    public static readonly DependencyProperty IconTextProperty =
        DependencyProperty.Register(nameof(IconText), typeof(string), typeof(SUICard), new PropertyMetadata("S"));

    /// <summary>Fluent 图标字形（可选，使用 Segoe Fluent Icons；非空且无 <see cref="IconSource"/> 时替换 <see cref="IconText"/>）。</summary>
    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }
    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(SUICard), new PropertyMetadata(null));

    /// <summary>左侧图标区域背景画刷（默认 #4C79FF）。</summary>
    public Brush IconColor
    {
        get => (Brush)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
    public static readonly DependencyProperty IconColorProperty =
        DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(SUICard),
            new PropertyMetadata(DefaultIconBrush));

    /// <summary>标题右侧状态徽标插槽（如 <see cref="SUIBadge"/>）。</summary>
    public object? BadgeContent
    {
        get => GetValue(BadgeContentProperty);
        set => SetValue(BadgeContentProperty, value);
    }
    public static readonly DependencyProperty BadgeContentProperty =
        DependencyProperty.Register(nameof(BadgeContent), typeof(object), typeof(SUICard), new PropertyMetadata(null));

    /// <summary>卡片右侧操作插槽（如按钮、箭头或快捷操作）。</summary>
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }
    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(SUICard), new PropertyMetadata(null));

    /// <summary>是否启用卡片点击交互与悬停/按下/焦点反馈；设置 <see cref="Command"/> 时自动视为可交互。</summary>
    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }
    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(SUICard),
            new PropertyMetadata(false, OnInteractivePropertyChanged));

    /// <summary>点击卡片或聚焦后按 Enter / Space 时执行的命令。</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SUICard),
            new PropertyMetadata(null, OnCommandChanged));

    /// <summary>传递给 <see cref="Command"/> 的参数。</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SUICard),
            new PropertyMetadata(null, OnCommandParameterOrTargetChanged));

    /// <summary><see cref="RoutedCommand"/> 的命令目标；未设置时以当前卡片作为目标。</summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(SUICard),
            new PropertyMetadata(null, OnCommandParameterOrTargetChanged));

    private static readonly DependencyPropertyKey IsClickablePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsClickable), typeof(bool), typeof(SUICard), new PropertyMetadata(false));

    /// <summary>当前卡片是否处于可点击交互状态（<see cref="IsInteractive"/> 为 true 或设置了 <see cref="Command"/>，且命令可执行）。</summary>
    public static readonly DependencyProperty IsClickableProperty = IsClickablePropertyKey.DependencyProperty;
    /// <inheritdoc cref="IsClickableProperty" />
    public bool IsClickable => (bool)GetValue(IsClickableProperty);

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPressed), typeof(bool), typeof(SUICard), new PropertyMetadata(false));

    /// <summary>当前卡片是否正被按下。</summary>
    public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;
    /// <inheritdoc cref="IsPressedProperty" />
    public bool IsPressed => (bool)GetValue(IsPressedProperty);

    /// <summary>卡片被点击或通过 Enter / Space 触发时引发。</summary>
    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SUICard));

    /// <summary>卡片被点击或通过 Enter / Space 激活时触发。</summary>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private static void OnInteractivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SUICard card) card.UpdateInteractionState();
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SUICard card) return;
        if (e.OldValue is ICommand oldCommand)
            CanExecuteChangedEventManager.RemoveHandler(oldCommand, card.OnCanExecuteChanged);
        if (e.NewValue is ICommand newCommand)
            CanExecuteChangedEventManager.AddHandler(newCommand, card.OnCanExecuteChanged);

        card.UpdateCanExecute();
    }

    private static void OnCommandParameterOrTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SUICard card) card.UpdateCanExecute();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => UpdateCanExecute();

    private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateCanExecute();

    private void UpdateCanExecute()
    {
        _canExecute = CanExecuteCommand();
        UpdateInteractionState();
    }

    private bool CanExecuteCommand()
    {
        var command = Command;
        if (command is null) return true;

        var parameter = CommandParameter;
        return command is RoutedCommand routed
            ? routed.CanExecute(parameter, CommandTarget ?? this)
            : command.CanExecute(parameter);
    }

    private void UpdateInteractionState()
    {
        var clickable = (IsInteractive || Command is not null) && _canExecute;
        SetValue(IsClickablePropertyKey, clickable);
        Focusable = clickable;
        IsTabStop = clickable;
        Cursor = clickable && IsEnabled ? Cursors.Hand : Cursors.Arrow;
        if (!clickable) SetValue(IsPressedPropertyKey, false);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsEnabledProperty)
        {
            Cursor = IsClickable && IsEnabled ? Cursors.Hand : Cursors.Arrow;
            if (!IsEnabled) SetValue(IsPressedPropertyKey, false);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!IsClickable || !IsEnabled || IsFromActionSlot(e.OriginalSource as DependencyObject)) return;

        Focus();
        CaptureMouse();
        SetValue(IsPressedPropertyKey, true);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsPressed) return;

        if (IsMouseCaptured) ReleaseMouseCapture();
        SetValue(IsPressedPropertyKey, false);

        if (IsClickable && IsEnabled && IsMouseOver && !IsFromActionSlot(e.OriginalSource as DependencyObject))
        {
            InvokeCard();
            e.Handled = true;
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        SetValue(IsPressedPropertyKey, false);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsClickable || !IsEnabled || !ReferenceEquals(e.OriginalSource, this)) return;

        if (e.Key == Key.Space)
        {
            SetValue(IsPressedPropertyKey, true);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            InvokeCard();
            e.Handled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Key.Space && IsPressed)
        {
            SetValue(IsPressedPropertyKey, false);
            if (IsClickable && IsEnabled) InvokeCard();
            e.Handled = true;
        }
    }

    private void InvokeCard()
    {
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));

        var command = Command;
        if (command == null) return;

        var parameter = CommandParameter;
        if (command is RoutedCommand routed)
        {
            var target = CommandTarget ?? this;
            if (routed.CanExecute(parameter, target)) routed.Execute(parameter, target);
        }
        else if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private bool IsFromActionSlot(DependencyObject? source)
    {
        if (source == null || ActionHost == null) return false;
        for (var current = source; current != null; current = GetParent(current))
        {
            if (ReferenceEquals(current, ActionHost)) return true;
            if (ReferenceEquals(current, this)) return false;
        }

        return false;
    }

    private static DependencyObject? GetParent(DependencyObject element) =>
        element is Visual
            ? VisualTreeHelper.GetParent(element)
            : LogicalTreeHelper.GetParent(element);

    protected override AutomationPeer OnCreateAutomationPeer() => new CardAutomationPeer(this);

    private sealed class CardAutomationPeer(SUICard owner) : UserControlAutomationPeer(owner), IInvokeProvider
    {
        public override object? GetPattern(PatternInterface patternInterface) =>
            patternInterface == PatternInterface.Invoke && ((SUICard)Owner).IsClickable
                ? this
                : base.GetPattern(patternInterface);

        protected override string GetClassNameCore() => nameof(SUICard);
        protected override AutomationControlType GetAutomationControlTypeCore() =>
            ((SUICard)Owner).IsClickable ? AutomationControlType.Button : AutomationControlType.Group;
        protected override string GetNameCore() =>
            base.GetNameCore() is { Length: > 0 } name ? name : ((SUICard)Owner).Title ?? string.Empty;

        public void Invoke()
        {
            var card = (SUICard)Owner;
            if (!IsEnabled() || !card.IsClickable)
                throw new ElementNotEnabledException();
            card.InvokeCard();
        }
    }
}
