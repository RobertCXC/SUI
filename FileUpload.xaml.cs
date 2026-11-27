using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace SUI;

/// <summary>文件选择完成事件参数。</summary>
public sealed class FilesSelectedEventArgs(RoutedEvent routedEvent, object source, IReadOnlyList<string> files)
    : RoutedEventArgs(routedEvent, source)
{
    /// <summary>本次选中后的文件路径列表。</summary>
    public IReadOnlyList<string> Files { get; } = files;
}

/// <summary>
/// 本地文件选择与拖放控件，封装 Win32 OpenFileDialog 过滤、单/多选与拖放校验。
/// </summary>
public partial class FileUpload : UserControl
{
    private static readonly char[] SemicolonSeparators = [';'];
    private bool _syncing;
    private bool _canAcceptDrag;

    public FileUpload()
    {
        InitializeComponent();
        var files = new ObservableCollection<string>();
        files.CollectionChanged += OnSelectedFilesCollectionChanged;
        SetValue(SelectedFilesPropertyKey, files);
        UpdateListVisibility();
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(FileUpload), new FrameworkPropertyMetadata("选择文件"));

    /// <summary>主标题。</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(FileUpload),
        new FrameworkPropertyMetadata("点击按钮选择文件，或将文件拖放到此区域"));

    /// <summary>辅助说明（支持显示允许的扩展名或大小提示）。</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
        nameof(ButtonText), typeof(string), typeof(FileUpload), new FrameworkPropertyMetadata("浏览文件"));

    /// <summary>选择按钮文案。</summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
        nameof(Filter), typeof(string), typeof(FileUpload), new FrameworkPropertyMetadata("所有文件 (*.*)|*.*"));

    /// <summary>Win32 OpenFileDialog 格式的过滤器，例如 "固件与文本 (*.bin;*.txt)|*.bin;*.txt|所有文件 (*.*)|*.*"。</summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public static readonly DependencyProperty InitialDirectoryProperty = DependencyProperty.Register(
        nameof(InitialDirectory), typeof(string), typeof(FileUpload), new FrameworkPropertyMetadata(null));

    /// <summary>打开文件对话框时的初始目录（可选）。</summary>
    public string? InitialDirectory
    {
        get => (string?)GetValue(InitialDirectoryProperty);
        set => SetValue(InitialDirectoryProperty, value);
    }

    public static readonly DependencyProperty AllowMultipleProperty = DependencyProperty.Register(
        nameof(AllowMultiple), typeof(bool), typeof(FileUpload), new FrameworkPropertyMetadata(false));

    /// <summary>是否允许一次选择多个文件。</summary>
    public bool AllowMultiple
    {
        get => (bool)GetValue(AllowMultipleProperty);
        set => SetValue(AllowMultipleProperty, value);
    }

    public static readonly DependencyProperty AllowDropFilesProperty = DependencyProperty.Register(
        nameof(AllowDropFiles), typeof(bool), typeof(FileUpload), new FrameworkPropertyMetadata(true));

    /// <summary>是否允许从资源管理器拖放文件。</summary>
    public bool AllowDropFiles
    {
        get => (bool)GetValue(AllowDropFilesProperty);
        set => SetValue(AllowDropFilesProperty, value);
    }

    public static readonly DependencyProperty SelectedFilePathProperty = DependencyProperty.Register(
        nameof(SelectedFilePath), typeof(string), typeof(FileUpload),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedFilePathChanged));

    /// <summary>当前选中的首个文件路径；无选中项时为 null。</summary>
    public string? SelectedFilePath
    {
        get => (string?)GetValue(SelectedFilePathProperty);
        set => SetValue(SelectedFilePathProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedFilesPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SelectedFiles), typeof(ObservableCollection<string>), typeof(FileUpload), new FrameworkPropertyMetadata(null));

    /// <summary>当前已选中的文件路径集合。</summary>
    public static readonly DependencyProperty SelectedFilesProperty = SelectedFilesPropertyKey.DependencyProperty;
    public ObservableCollection<string> SelectedFiles => (ObservableCollection<string>)GetValue(SelectedFilesProperty);

    public static readonly DependencyProperty FilesSelectedCommandProperty = DependencyProperty.Register(
        nameof(FilesSelectedCommand), typeof(ICommand), typeof(FileUpload), new FrameworkPropertyMetadata(null));

    /// <summary>文件选择变化后执行的命令，参数为当前文件路径列表（IReadOnlyList&lt;string&gt;）。</summary>
    public ICommand? FilesSelectedCommand
    {
        get => (ICommand?)GetValue(FilesSelectedCommandProperty);
        set => SetValue(FilesSelectedCommandProperty, value);
    }

    private static readonly DependencyPropertyKey IsDragOverPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsDragOver), typeof(bool), typeof(FileUpload), new FrameworkPropertyMetadata(false));

    /// <summary>当前是否有文件拖拽悬停在控件上方。</summary>
    public static readonly DependencyProperty IsDragOverProperty = IsDragOverPropertyKey.DependencyProperty;
    public bool IsDragOver => (bool)GetValue(IsDragOverProperty);

    public static readonly RoutedEvent FilesSelectedEvent = EventManager.RegisterRoutedEvent(
        nameof(FilesSelected), RoutingStrategy.Bubble, typeof(EventHandler<FilesSelectedEventArgs>), typeof(FileUpload));

    /// <summary>用户通过对话框或拖放选择文件后触发。</summary>
    public event EventHandler<FilesSelectedEventArgs> FilesSelected
    {
        add => AddHandler(FilesSelectedEvent, value);
        remove => RemoveHandler(FilesSelectedEvent, value);
    }

    /// <summary>打开系统文件选择对话框。</summary>
    public bool Browse()
    {
        if (!IsEnabled) return false;

        var dialog = new OpenFileDialog
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "选择文件" : Title,
            Filter = string.IsNullOrWhiteSpace(Filter) ? "所有文件 (*.*)|*.*" : Filter,
            Multiselect = AllowMultiple,
        };
        if (!string.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
            dialog.InitialDirectory = InitialDirectory;

        var owner = Window.GetWindow(this);
        var ok = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        if (ok != true || dialog.FileNames.Length == 0) return false;

        ApplyFiles(dialog.FileNames);
        return true;
    }

    /// <summary>清空已选文件。</summary>
    public void Clear()
    {
        if (SelectedFiles.Count == 0 && SelectedFilePath == null) return;
        _syncing = true;
        try
        {
            SelectedFiles.Clear();
            SetCurrentValue(SelectedFilePathProperty, null);
            UpdateListVisibility();
        }
        finally
        {
            _syncing = false;
        }
        NotifySelectionChanged();
    }

    private static void OnSelectedFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FileUpload)d;
        if (control._syncing) return;

        control._syncing = true;
        try
        {
            control.SelectedFiles.Clear();
            if (e.NewValue is string path && !string.IsNullOrWhiteSpace(path))
                control.SelectedFiles.Add(path);
            control.UpdateListVisibility();
        }
        finally
        {
            control._syncing = false;
        }
    }

    private void OnSelectedFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            SetCurrentValue(SelectedFilePathProperty, SelectedFiles.Count > 0 ? SelectedFiles[0] : null);
            UpdateListVisibility();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void ApplyFiles(IEnumerable<string> paths)
    {
        var valid = paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (valid.Count == 0) return;

        _syncing = true;
        try
        {
            if (AllowMultiple)
            {
                foreach (var path in valid)
                {
                    if (!SelectedFiles.Contains(path, StringComparer.OrdinalIgnoreCase))
                        SelectedFiles.Add(path);
                }
            }
            else
            {
                SelectedFiles.Clear();
                SelectedFiles.Add(valid[0]);
            }

            SetCurrentValue(SelectedFilePathProperty, SelectedFiles.Count > 0 ? SelectedFiles[0] : null);
            UpdateListVisibility();
        }
        finally
        {
            _syncing = false;
        }

        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        var snapshot = Array.AsReadOnly<string>([.. SelectedFiles]);
        RaiseEvent(new FilesSelectedEventArgs(FilesSelectedEvent, this, snapshot));

        var command = FilesSelectedCommand;
        if (command != null && command.CanExecute(snapshot))
            command.Execute(snapshot);
    }

    private void UpdateListVisibility()
    {
        var hasFiles = SelectedFiles != null && SelectedFiles.Count > 0;
        SelectedFilesList.Visibility = hasFiles ? Visibility.Visible : Visibility.Collapsed;
        ClearButton.Visibility = hasFiles ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e) => Browse();

    private void OnClearClick(object sender, RoutedEventArgs e) => Clear();

    private void OnRemoveFileClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string path })
        {
            SelectedFiles.Remove(path);
            NotifySelectionChanged();
        }
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);
        _canAcceptDrag = IsEnabled && AllowDropFiles && HasAcceptableFile(e.Data);
        ApplyDragEffects(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);
        ApplyDragEffects(e);
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);
        _canAcceptDrag = false;
        SetValue(IsDragOverPropertyKey, false);
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        _canAcceptDrag = false;
        SetValue(IsDragOverPropertyKey, false);

        if (!IsEnabled || !AllowDropFiles) return;
        var matched = ExtractMatchingFiles(e.Data);
        if (matched.Count > 0)
        {
            ApplyFiles(matched);
            e.Handled = true;
        }
    }

    private void ApplyDragEffects(DragEventArgs e)
    {
        var acceptable = IsEnabled && AllowDropFiles && _canAcceptDrag;
        e.Effects = acceptable ? DragDropEffects.Copy : DragDropEffects.None;
        SetValue(IsDragOverPropertyKey, acceptable);
        e.Handled = true;
    }

    private bool HasAcceptableFile(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop)) return false;
        if (data.GetData(DataFormats.FileDrop) is not string[] dropped) return false;

        var allowed = ParseAllowedExtensions(Filter);
        foreach (var path in dropped)
        {
            if (IsMatchingFile(path, allowed))
                return true;
        }

        return false;
    }

    private List<string> ExtractMatchingFiles(IDataObject data)
    {
        var result = new List<string>();
        if (!data.GetDataPresent(DataFormats.FileDrop)) return result;
        if (data.GetData(DataFormats.FileDrop) is not string[] dropped) return result;

        var allowed = ParseAllowedExtensions(Filter);
        foreach (var path in dropped)
        {
            if (!IsMatchingFile(path, allowed)) continue;
            result.Add(path);
            if (!AllowMultiple) break;
        }

        return result;
    }

    private static bool IsMatchingFile(string path, string[]? allowed)
    {
        if (!File.Exists(path)) return false;
        if (allowed is null) return true;

        var fileName = Path.GetFileName(path);
        foreach (var ext in allowed)
        {
            if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string[]? ParseAllowedExtensions(string? filter)
    {
        if (filter is null || string.IsNullOrWhiteSpace(filter)) return null;
        var parts = filter.Split('|');
        if (parts.Length < 2) return null;

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < parts.Length; i += 2)
        {
            var isFirstGroup = i == 1;
            foreach (var pattern in parts[i].Split(SemicolonSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = pattern.Trim();
                if (trimmed is "*.*" or "*")
                {
                    if (isFirstGroup) return null;
                    continue;
                }
                if (trimmed.StartsWith("*.", StringComparison.Ordinal) && trimmed.Length > 2)
                    extensions.Add(trimmed.Substring(1));
            }
        }

        return extensions.Count > 0 ? [.. extensions] : null;
    }
}
