using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Models.Services.NewsFeed;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Views.NewsFeed;

internal partial class NewsItem : UserControl
{
    public static readonly StyledProperty<News> NewsProperty =
        AvaloniaProperty.Register<NewsItem, News>(
        nameof(News));

    public News News
    {
        get { return (News)GetValue(NewsProperty); }
        set { SetValue(NewsProperty, value); }
    }
    
    public NewsItem()
    {
        InitializeComponent();
    }

    private void CoverImageClicked(object sender, PointerPressedEventArgs e)
    {
        IOperatingSystem.Current.OpenUri(News.Url);
    }
}

