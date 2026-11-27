using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SUI;

/// <summary>流程步骤状态。</summary>
public enum StepStatus
{
    Pending,
    Active,
    Completed,
    Error,
}

/// <summary>流程步骤数据项，可直接绑定到 <see cref="ItemsControl.ItemsSource"/> 或在 XAML 中声明。</summary>
public class SUIStepItem : INotifyPropertyChanged
{
    private string _key = string.Empty;
    private string _title = string.Empty;
    private string? _description;
    private string? _iconGlyph;
    private StepStatus _status = StepStatus.Pending;
    private bool _isEnabled = true;
    private bool? _isClickable;
    private string? _toolTip;

    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string? IconGlyph
    {
        get => _iconGlyph;
        set => SetField(ref _iconGlyph, value);
    }

    public StepStatus Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    /// <summary>可选覆盖单个步骤是否允许点击；为 null 时遵循 SUIStepper.AllowCompletedClick。</summary>
    public bool? IsClickable
    {
        get => _isClickable;
        set => SetField(ref _isClickable, value);
    }

    public string? ToolTip
    {
        get => _toolTip;
        set => SetField(ref _toolTip, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>步骤被点击时触发的事件参数。</summary>
public sealed class SUIStepClickedEventArgs(
    RoutedEvent routedEvent,
    object source,
    object? item,
    int index,
    int stepNumber) : RoutedEventArgs(routedEvent, source)
{
    public object? Item { get; } = item;
    public int Index { get; } = index;
    public int StepNumber { get; } = stepNumber;
}

/// <summary>SUIStepper 单个步骤的容器控件。</summary>
public class SUIStepperItem : ContentControl
{
    private bool _isPressed;

    static SUIStepperItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIStepperItem), new FrameworkPropertyMetadata(typeof(SUIStepperItem)));
        FocusableProperty.OverrideMetadata(typeof(SUIStepperItem), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUIStepperItem), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty StepNumberProperty = DependencyProperty.Register(
        nameof(StepNumber), typeof(int), typeof(SUIStepperItem), new FrameworkPropertyMetadata(1));

    public int StepNumber
    {
        get => (int)GetValue(StepNumberProperty);
        set => SetValue(StepNumberProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SUIStepperItem), new FrameworkPropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SUIStepperItem), new FrameworkPropertyMetadata(null));

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IconGlyphProperty = DependencyProperty.Register(
        nameof(IconGlyph), typeof(string), typeof(SUIStepperItem), new FrameworkPropertyMetadata(null));

    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status), typeof(StepStatus), typeof(SUIStepperItem),
        new FrameworkPropertyMetadata(StepStatus.Pending, OnStatusChanged),
        value => Enum.IsDefined(typeof(StepStatus), value));

    public StepStatus Status
    {
        get => (StepStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty IsFirstProperty = DependencyProperty.Register(
        nameof(IsFirst), typeof(bool), typeof(SUIStepperItem), new FrameworkPropertyMetadata(false));

    public bool IsFirst
    {
        get => (bool)GetValue(IsFirstProperty);
        set => SetValue(IsFirstProperty, value);
    }

    public static readonly DependencyProperty IsLastProperty = DependencyProperty.Register(
        nameof(IsLast), typeof(bool), typeof(SUIStepperItem), new FrameworkPropertyMetadata(false));

    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        set => SetValue(IsLastProperty, value);
    }

    public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
        nameof(IsClickable), typeof(bool), typeof(SUIStepperItem),
        new FrameworkPropertyMetadata(false, OnIsClickableChanged));

    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(SUIStepperItem),
        new FrameworkPropertyMetadata(Orientation.Horizontal));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    internal SUIStepper? OwnerStepper { get; set; }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!IsClickable || !IsEnabled) return;
        _isPressed = CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        var wasPressed = _isPressed;
        _isPressed = false;
        if (IsMouseCaptured) ReleaseMouseCapture();

        if (!wasPressed || !IsClickable || !IsEnabled) return;
        var pos = e.GetPosition(this);
        if (pos.X >= 0 && pos.X <= ActualWidth && pos.Y >= 0 && pos.Y <= ActualHeight)
        {
            OwnerStepper?.NotifyStepClicked(this);
            e.Handled = true;
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        _isPressed = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsClickable || !IsEnabled) return;
        if (e.Key is Key.Enter or Key.Space)
        {
            OwnerStepper?.NotifyStepClicked(this);
            e.Handled = true;
        }
    }

    private static void OnIsClickableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (SUIStepperItem)d;
        var clickable = (bool)e.NewValue;
        item.Focusable = clickable;
        item.IsTabStop = clickable;
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SUIStepperItem item)
        {
            item.OwnerStepper?.UpdateContainerClickability(item);
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new StepperItemAutomationPeer(this);

    private sealed class StepperItemAutomationPeer(SUIStepperItem owner)
        : FrameworkElementAutomationPeer(owner), IInvokeProvider
    {
        protected override string GetClassNameCore() => nameof(SUIStepperItem);
        protected override AutomationControlType GetAutomationControlTypeCore() =>
            owner.IsClickable ? AutomationControlType.Button : AutomationControlType.ListItem;

        protected override string GetNameCore()
        {
            if (base.GetNameCore() is { Length: > 0 } name) return name;
            return $"{owner.StepNumber}. {owner.Title} ({owner.Status})".Trim();
        }

        public override object? GetPattern(PatternInterface patternInterface) =>
            patternInterface == PatternInterface.Invoke && owner.IsClickable && owner.IsEnabled
                ? this
                : base.GetPattern(patternInterface);

        public void Invoke()
        {
            if (!IsEnabled()) throw new ElementNotEnabledException();
            if (owner.IsClickable)
            {
                owner.OwnerStepper?.NotifyStepClicked(owner);
            }
        }
    }
}

/// <summary>轻量流程步骤条 / 向导状态指示控件，支持四态展示与已完成步骤回看。</summary>
public class SUIStepper : ItemsControl
{
    private readonly Dictionary<SUIStepperItem, SUIStepItem> _watchedItems = [];

    static SUIStepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIStepper), new FrameworkPropertyMetadata(typeof(SUIStepper)));
        FocusableProperty.OverrideMetadata(typeof(SUIStepper), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUIStepper), new FrameworkPropertyMetadata(false));
    }

    public SUIStepper()
    {
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(SUIStepper),
        new FrameworkPropertyMetadata(Orientation.Horizontal, OnLayoutOrStateChanged));

    /// <summary>步骤条排列方向（Horizontal / Vertical）。</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty CurrentStepIndexProperty = DependencyProperty.Register(
        nameof(CurrentStepIndex), typeof(int), typeof(SUIStepper),
        new FrameworkPropertyMetadata(-1, OnLayoutOrStateChanged));

    /// <summary>当前步骤（0 起始）。设为 >= 0 时自动计算步骤四态；设为 -1 时直接使用每项 SUIStepItem.Status。</summary>
    public int CurrentStepIndex
    {
        get => (int)GetValue(CurrentStepIndexProperty);
        set => SetValue(CurrentStepIndexProperty, value);
    }

    public static readonly DependencyProperty IsCurrentStepErrorProperty = DependencyProperty.Register(
        nameof(IsCurrentStepError), typeof(bool), typeof(SUIStepper),
        new FrameworkPropertyMetadata(false, OnLayoutOrStateChanged));

    /// <summary>配合 CurrentStepIndex 使用，指示当前步骤是否处于错误状态。</summary>
    public bool IsCurrentStepError
    {
        get => (bool)GetValue(IsCurrentStepErrorProperty);
        set => SetValue(IsCurrentStepErrorProperty, value);
    }

    public static readonly DependencyProperty AllowCompletedClickProperty = DependencyProperty.Register(
        nameof(AllowCompletedClick), typeof(bool), typeof(SUIStepper),
        new FrameworkPropertyMetadata(false, OnLayoutOrStateChanged));

    /// <summary>是否允许点击状态为 Completed 的步骤以回看。</summary>
    public bool AllowCompletedClick
    {
        get => (bool)GetValue(AllowCompletedClickProperty);
        set => SetValue(AllowCompletedClickProperty, value);
    }

    public static readonly DependencyProperty StepClickCommandProperty = DependencyProperty.Register(
        nameof(StepClickCommand), typeof(ICommand), typeof(SUIStepper), new FrameworkPropertyMetadata(null));

    /// <summary>点击可交互步骤时执行的命令，参数为对应数据项。</summary>
    public ICommand? StepClickCommand
    {
        get => (ICommand?)GetValue(StepClickCommandProperty);
        set => SetValue(StepClickCommandProperty, value);
    }

    public static readonly RoutedEvent StepClickedEvent = EventManager.RegisterRoutedEvent(
        nameof(StepClicked), RoutingStrategy.Bubble,
        typeof(EventHandler<SUIStepClickedEventArgs>), typeof(SUIStepper));

    /// <summary>点击可交互步骤时触发。</summary>
    public event EventHandler<SUIStepClickedEventArgs> StepClicked
    {
        add => AddHandler(StepClickedEvent, value);
        remove => RemoveHandler(StepClickedEvent, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SUIStepperItem;

    protected override DependencyObject GetContainerForItemOverride() => new SUIStepperItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not SUIStepperItem container) return;

        container.OwnerStepper = this;
        if (item is SUIStepItem step)
        {
            container.SetBinding(SUIStepperItem.TitleProperty, new Binding(nameof(SUIStepItem.Title)) { Source = step });
            container.SetBinding(SUIStepperItem.DescriptionProperty, new Binding(nameof(SUIStepItem.Description)) { Source = step });
            container.SetBinding(SUIStepperItem.IconGlyphProperty, new Binding(nameof(SUIStepItem.IconGlyph)) { Source = step });
            container.SetBinding(IsEnabledProperty, new Binding(nameof(SUIStepItem.IsEnabled)) { Source = step });
            container.SetBinding(ToolTipProperty, new Binding(nameof(SUIStepItem.ToolTip)) { Source = step });
            if (CurrentStepIndex < 0)
            {
                container.SetBinding(SUIStepperItem.StatusProperty, new Binding(nameof(SUIStepItem.Status)) { Source = step });
            }

            if (_watchedItems.TryGetValue(container, out var previous) && !ReferenceEquals(previous, step))
            {
                previous.PropertyChanged -= OnStepItemPropertyChanged;
            }
            step.PropertyChanged -= OnStepItemPropertyChanged;
            step.PropertyChanged += OnStepItemPropertyChanged;
            _watchedItems[container] = step;
        }
        else if (item is not SUIStepperItem && string.IsNullOrEmpty(container.Title) && ItemTemplate is null)
        {
            container.Title = item?.ToString() ?? string.Empty;
        }

        // 未配置 ItemTemplate 时清空 WPF 默认填入的数据对象 Content，避免将 SUIStepItem 类名渲染到步骤副标题下方。
        if (item is not SUIStepperItem && ItemTemplate is null && ItemTemplateSelector is null)
        {
            container.ClearValue(ContentControl.ContentProperty);
        }

        SyncContainers();
    }

    protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
        SyncItemContents();
    }

    protected override void OnItemTemplateSelectorChanged(DataTemplateSelector oldItemTemplateSelector, DataTemplateSelector newItemTemplateSelector)
    {
        base.OnItemTemplateSelectorChanged(oldItemTemplateSelector, newItemTemplateSelector);
        SyncItemContents();
    }

    private void SyncItemContents()
    {
        var hasTemplate = ItemTemplate is not null || ItemTemplateSelector is not null;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item is SUIStepperItem) continue;
            if (ItemContainerGenerator.ContainerFromIndex(i) is not SUIStepperItem container) continue;

            if (hasTemplate)
            {
                container.Content = item;
            }
            else
            {
                container.ClearValue(ContentControl.ContentProperty);
            }
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is not SUIStepperItem container) return;

        container.OwnerStepper = null;
        if (_watchedItems.TryGetValue(container, out var watched))
        {
            watched.PropertyChanged -= OnStepItemPropertyChanged;
            _watchedItems.Remove(container);
        }
        BindingOperations.ClearAllBindings(container);
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        SyncContainers();
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
        {
            SyncContainers();
        }
    }

    private void OnStepItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SUIStepItem.Status) or nameof(SUIStepItem.IsClickable))
        {
            SyncContainers();
        }
    }

    internal void UpdateContainerClickability(SUIStepperItem container)
    {
        var index = ItemContainerGenerator.IndexFromContainer(container);
        if (index < 0) return;

        var item = Items[index];
        var explicitClickable = (item as SUIStepItem)?.IsClickable;
        container.IsClickable = explicitClickable ?? (AllowCompletedClick && container.Status == StepStatus.Completed);
    }

    internal void NotifyStepClicked(SUIStepperItem container)
    {
        var index = ItemContainerGenerator.IndexFromContainer(container);
        if (index < 0 || index >= Items.Count) return;

        var item = Items[index];
        RaiseEvent(new SUIStepClickedEventArgs(StepClickedEvent, this, item, index, index + 1));

        if (StepClickCommand != null && StepClickCommand.CanExecute(item))
        {
            StepClickCommand.Execute(item);
        }
    }

    private void SyncContainers()
    {
        var count = Items.Count;
        var current = CurrentStepIndex;
        var currentError = IsCurrentStepError;

        for (var i = 0; i < count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is not SUIStepperItem container) continue;

            container.StepNumber = i + 1;
            container.IsFirst = i == 0;
            container.IsLast = i == count - 1;
            container.Orientation = Orientation;

            var item = Items[i];
            if (current >= 0)
            {
                var computed = i < current
                    ? StepStatus.Completed
                    : (i == current ? (currentError ? StepStatus.Error : StepStatus.Active) : StepStatus.Pending);

                BindingOperations.ClearBinding(container, SUIStepperItem.StatusProperty);
                container.SetCurrentValue(SUIStepperItem.StatusProperty, computed);
            }
            else if (item is SUIStepItem step)
            {
                if (BindingOperations.GetBinding(container, SUIStepperItem.StatusProperty) is null)
                {
                    container.SetBinding(SUIStepperItem.StatusProperty, new Binding(nameof(SUIStepItem.Status)) { Source = step });
                }
            }

            UpdateContainerClickability(container);
        }
    }

    private static void OnLayoutOrStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SUIStepper)d).SyncContainers();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new StepperAutomationPeer(this);

    private sealed class StepperAutomationPeer(SUIStepper owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(SUIStepper);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.List;
    }
}
