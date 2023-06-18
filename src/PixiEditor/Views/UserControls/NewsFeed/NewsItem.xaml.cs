using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.Services.NewsFeed;

namespace PixiEditor.Views.UserControls.NewsFeed;

internal partial class NewsItem : UserControl
{
    public static readonly DependencyProperty NewsProperty = DependencyProperty.Register(
        nameof(News), typeof(News), typeof(NewsItem), new PropertyMetadata(default(News)));

    public News News
    {
        get { return (News)GetValue(NewsProperty); }
        set { SetValue(NewsProperty, value); }
    }
    public NewsItem()
    {
        InitializeComponent();
    }
}

