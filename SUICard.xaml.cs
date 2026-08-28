using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SUI
{
    /// <summary>
    /// SUICard.xaml 的交互逻辑
    /// </summary>
    public partial class SUICard : UserControl
    {
        public SUICard()
        {
            InitializeComponent();
        }

        // 图片来源（可为空）。默认不设置时，用XAML里的蓝底字母M作为默认图示
        public ImageSource? IconSource
        {
            get => (ImageSource?)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(SUICard));

        // 标题（默认：标题）
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SUICard), new PropertyMetadata("标题"));

        // 描述（略）
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(SUICard), new PropertyMetadata("略"));

        // 图标文字（默认：S）
        public string IconText
        {
            get => (string)GetValue(IconTextProperty);
            set => SetValue(IconTextProperty, value);
        }
        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register(nameof(IconText), typeof(string), typeof(SUICard), new PropertyMetadata("S"));

        // 图标颜色（默认：#4C79FF）
        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }
        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(SUICard), 
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C79FF"))));
    }
}
