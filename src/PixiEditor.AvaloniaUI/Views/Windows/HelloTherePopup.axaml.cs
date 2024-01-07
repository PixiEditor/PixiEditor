using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Services.NewsFeed;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.AvaloniaUI.Models.UserData;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AvaloniaUI.Views.Windows;

/// <summary>
/// Interaction logic for HelloTherePopup.xaml.
/// </summary>
internal partial class HelloTherePopup : Window
{
    public RecentlyOpenedCollection RecentlyOpened { get => FileViewModel.RecentlyOpened; }

    public static readonly StyledProperty<FileViewModel> FileViewModelProperty =
        AvaloniaProperty.Register<HelloTherePopup, FileViewModel>(nameof(FileViewModel));

    public static readonly StyledProperty<bool> RecentlyOpenedEmptyProperty =
        AvaloniaProperty.Register<HelloTherePopup, bool>(nameof(RecentlyOpenedEmpty));

    public static readonly StyledProperty<bool> IsFetchingNewsProperty =
        AvaloniaProperty.Register<HelloTherePopup, bool>(nameof(IsFetchingNews), defaultValue: default(bool));

    public static readonly StyledProperty<bool> NewsPanelCollapsedProperty =
        AvaloniaProperty.Register<HelloTherePopup, bool>(nameof(NewsPanelCollapsed), defaultValue: false);

    public static readonly StyledProperty<bool> FailedFetchingNewsProperty =
        AvaloniaProperty.Register<HelloTherePopup, bool>(nameof(FailedFetchingNews), defaultValue: false);

    static HelloTherePopup()
    {
        NewsPanelCollapsedProperty.Changed.Subscribe(NewsPanelCollapsedChangedCallback);
    }

    public bool NewsPanelCollapsed
    {
        get { return (bool)GetValue(NewsPanelCollapsedProperty); }
        set { SetValue(NewsPanelCollapsedProperty, value); }
    }
    public bool IsFetchingNews
    {
        get { return (bool)GetValue(IsFetchingNewsProperty); }
        set { SetValue(IsFetchingNewsProperty, value); }
    }

    public bool FailedFetchingNews
    {
        get { return (bool)GetValue(FailedFetchingNewsProperty); }
        set { SetValue(FailedFetchingNewsProperty, value); }
    }

    public ObservableRangeCollection<News> News { get; set; } = new ObservableRangeCollection<News>();

    public static string VersionText =>
        $"v{VersionHelpers.GetCurrentAssemblyVersionString()}";

    public FileViewModel FileViewModel { get => (FileViewModel)GetValue(FileViewModelProperty); set => SetValue(FileViewModelProperty, value); }

    public bool RecentlyOpenedEmpty { get => (bool)GetValue(RecentlyOpenedEmptyProperty); set => SetValue(RecentlyOpenedEmptyProperty, value); }

    public AsyncRelayCommand OpenFileCommand { get; set; }

    public AsyncRelayCommand OpenNewFileCommand { get; set; }

    public RelayCommand<string> OpenRecentCommand { get; set; }

    public RelayCommand<string> OpenInExplorerCommand { get; set; }

    public bool IsClosing { get; private set; }

    private NewsProvider NewsProvider { get; set; }

    public bool NewsDisabled => _newsDisabled;

    public bool ShowDonateButton => // Steam doesn't allow external donations :(
#if STEAM
        false;
#else
        true;
#endif

    private bool _newsDisabled = false;

    public HelloTherePopup(FileViewModel fileViewModel)
    {
        DataContext = this;
        FileViewModel = fileViewModel;

        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        OpenNewFileCommand = new AsyncRelayCommand(OpenNewFile);
        OpenRecentCommand = new RelayCommand<string>(OpenRecent);
        OpenInExplorerCommand = new RelayCommand<string>(OpenInExplorer, CanOpenInExplorer);

        RecentlyOpenedEmpty = RecentlyOpened.Count == 0;
        RecentlyOpened.CollectionChanged += RecentlyOpened_CollectionChanged;

        _newsDisabled = IPreferences.Current.GetPreference<bool>(PreferencesConstants.DisableNewsPanel);

        NewsProvider = new NewsProvider();

        Closing += (_, _) => { IsClosing = true; };

        InitializeComponent();

        int newsWidth = 300;

        NewsPanelCollapsed = IPreferences.Current.GetPreference<bool>(PreferencesConstants.NewsPanelCollapsed);

        if (_newsDisabled || NewsPanelCollapsed)
        {
            Enumerable.Last<ColumnDefinition>(grid.ColumnDefinitions).Width = new GridLength(0);
            newsWidth = 0;
        }

        if (RecentlyOpenedEmpty)
        {
            Width = 500 + newsWidth;
            Height = 500;
        }
        else if (RecentlyOpened.Count < 4)
        {
            Width = 545 + newsWidth;
            Height = 500;
        }
        else if (RecentlyOpened.Count < 7)
        {
            Width = 575 + newsWidth;
            Height = 670;
        }
    }

    private static void NewsPanelCollapsedChangedCallback(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        HelloTherePopup helloTherePopup = (HelloTherePopup)e.Sender;

        if(helloTherePopup._newsDisabled)
            return;

        if (e.NewValue.Value)
        {
            helloTherePopup.Width -= 300;
            Enumerable.Last<ColumnDefinition>(helloTherePopup.grid.ColumnDefinitions).Width = new GridLength(0);
        }
        else
        {
            helloTherePopup.Width += 300;
            Enumerable.Last<ColumnDefinition>(helloTherePopup.grid.ColumnDefinitions).Width = new GridLength(300);
        }

        IPreferences.Current.UpdatePreference(PreferencesConstants.NewsPanelCollapsed, e.NewValue.Value);
    }

    private void RecentlyOpened_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RecentlyOpenedEmpty = FileViewModel.RecentlyOpened.Count == 0;
    }

    private async Task OpenFile()
    {
        Application.Current.ForDesktopMainWindow(mainWindow => mainWindow.Activate());
        Close();
        await FileViewModel.OpenFromOpenFileDialog();
    }

    private async Task OpenNewFile()
    {
        Application.Current.ForDesktopMainWindow(mainWindow => mainWindow.Activate());
        Close();
        await FileViewModel.CreateFromNewFileDialog();
    }

    private void OpenRecent(string parameter)
    {
        Application.Current.ForDesktopMainWindow(mainWindow => mainWindow.Activate());
        Close();
        FileViewModel.OpenRecent(parameter);
    }

    private void OpenInExplorer(string parameter)
    {
        IOperatingSystem.Current.OpenFolder(parameter);
    }

    private bool CanOpenInExplorer(string parameter) => File.Exists(parameter);

    private async void HelloTherePopup_OnLoaded(object sender, RoutedEventArgs e)
    {
        return; // TODO
        if(_newsDisabled) return;

        try
        {
            IsFetchingNews = true;
            var news = await NewsProvider.FetchNewsAsync();
            if (news is not null)
            {
                IsFetchingNews = false;
                News.Clear();
                News.AddRange(news);
                if (NewsPanelCollapsed && News.Any(x => x.IsNew))
                {
                    NewsPanelCollapsed = false;
                }
            }
            else
            {
                IsFetchingNews = false;
                FailedFetchingNews = true;
            }
        }
        catch(Exception ex)
        {
            IsFetchingNews = false;
            FailedFetchingNews = true;
            await CrashHelper.SendExceptionInfoToWebhookAsync(ex);
        }
    }
}
