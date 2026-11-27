using System.Windows;
using System.Windows.Controls;

namespace SUI;

/// <summary>为 TextBox、PasswordBox 与 DatePicker 提供占位提示文字。</summary>
public static class TextBoxAssist
{
    /// <summary>输入内容为空时显示的占位提示；为 null 或空字符串时不显示。</summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.RegisterAttached("PlaceholderText", typeof(string), typeof(TextBoxAssist),
            new FrameworkPropertyMetadata(null, OnPlaceholderTextChanged));

    public static string? GetPlaceholderText(DependencyObject element) =>
        (string?)element.GetValue(PlaceholderTextProperty);

    public static void SetPlaceholderText(DependencyObject element, string? value) =>
        element.SetValue(PlaceholderTextProperty, value);

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("IsEmpty", typeof(bool), typeof(TextBoxAssist),
            new FrameworkPropertyMetadata(true));

    /// <summary>PasswordBox 当前是否没有输入内容（只读，由 SUI 在设置占位提示后维护）。</summary>
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public static bool GetIsEmpty(DependencyObject element) => (bool)element.GetValue(IsEmptyProperty);

    private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox) return;

        passwordBox.PasswordChanged -= OnPasswordChanged;
        if (e.NewValue is string { Length: > 0 })
            passwordBox.PasswordChanged += OnPasswordChanged;
        UpdatePasswordState(passwordBox);
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e) =>
        UpdatePasswordState((PasswordBox)sender);

    private static void UpdatePasswordState(PasswordBox passwordBox)
    {
        using var password = passwordBox.SecurePassword;
        passwordBox.SetValue(IsEmptyPropertyKey, password.Length == 0);
    }
}
