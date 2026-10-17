using System.Windows.Controls;

namespace SUI.Gallery.Pages;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
#if NETFRAMEWORK
        const string target = ".NET Framework 4.8";
#elif NET8_0
        const string target = ".NET 8";
#else
        const string target = ".NET 10";
#endif
        RuntimeInfo.Text = $"当前展示目标：{target} · 支持 Windows 10 / 11";
    }
}
