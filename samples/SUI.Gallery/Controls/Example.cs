using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace SUI.Gallery.Controls;

/// <summary>Groups a live preview with its description and interaction hints.</summary>
public class Example : ContentControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(Example));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Example));
    public static readonly DependencyProperty HintProperty = DependencyProperty.Register(nameof(Hint), typeof(string), typeof(Example));

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public string Hint { get => (string)GetValue(HintProperty); set => SetValue(HintProperty, value); }

    protected override AutomationPeer OnCreateAutomationPeer() => new ExampleAutomationPeer(this);
}

internal sealed class ExampleAutomationPeer(Example owner) : FrameworkElementAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
    protected override string GetClassNameCore() => nameof(Example);
    protected override string GetNameCore() => ((Example)Owner).Title ?? base.GetNameCore();
}
