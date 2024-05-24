using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PixiEditor.Exceptions;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Services.NewsFeed;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for HelloTherePopup.xaml.
/// </summary>
internal partial class HelloTherePopup : Window
{
    public RecentlyOpenedCollection RecentlyOpened { get => FileViewModel.RecentlyOpened; }

    public static readonly DependencyProperty FileViewModelProperty =
        DependencyProperty.Register(nameof(FileViewModel), typeof(FileViewModel), typeof(HelloTherePopup));

    public static readonly DependencyProperty RecentlyOpenedEmptyProperty =
        DependencyProperty.Register(nameof(RecentlyOpenedEmpty), typeof(bool), typeof(HelloTherePopup));

    public static readonly DependencyProperty IsFetchingNewsProperty = DependencyProperty.Register(
        nameof(IsFetchingNews), typeof(bool), typeof(HelloTherePopup), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty NewsPanelCollapsedProperty = DependencyProperty.Register(
        nameof(NewsPanelCollapsed), typeof(bool), typeof(HelloTherePopup),
        new PropertyMetadata(false, NewsPanelCollapsedChangedCallback));

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

    public static readonly DependencyProperty FailedFetchingNewsProperty = DependencyProperty.Register(
        nameof(FailedFetchingNews), typeof(bool), typeof(HelloTherePopup), new PropertyMetadata(false));

    public bool FailedFetchingNews
    {
        get { return (bool)GetValue(FailedFetchingNewsProperty); }
        set { SetValue(FailedFetchingNewsProperty, value); }
    }

    public WpfObservableRangeCollection<News> News { get; set; } = new WpfObservableRangeCollection<News>();

    public static string VersionText =>
        $"v{VersionHelpers.GetCurrentAssemblyVersionString()}";

    public FileViewModel FileViewModel { get => (FileViewModel)GetValue(FileViewModelProperty); set => SetValue(FileViewModelProperty, value); }

    public bool RecentlyOpenedEmpty { get => (bool)GetValue(RecentlyOpenedEmptyProperty); set => SetValue(RecentlyOpenedEmptyProperty, value); }

    public RelayCommand OpenFileCommand { get; set; }

    public RelayCommand OpenNewFileCommand { get; set; }

    public RelayCommand OpenRecentCommand { get; set; }

    public RelayCommand OpenInExplorerCommand { get; set; }

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
        Owner = Application.Current.MainWindow;
        FileViewModel = fileViewModel;

        OpenFileCommand = new RelayCommand(OpenFile);
        OpenNewFileCommand = new RelayCommand(OpenNewFile);
        OpenRecentCommand = new RelayCommand(OpenRecent);
        OpenInExplorerCommand = new RelayCommand(OpenInExplorer, CanOpenInExplorer);

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
            newsColumn.Width = new GridLength(0);
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

    private static void NewsPanelCollapsedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        HelloTherePopup helloTherePopup = (HelloTherePopup)d;

        if(helloTherePopup._newsDisabled || e.NewValue is not bool newValue)
            return;

        if (newValue)
        {
            helloTherePopup.Width -= 300;
            helloTherePopup.newsColumn.Width = new GridLength(0);
        }
        else
        {
            helloTherePopup.Width += 300;
            helloTherePopup.newsColumn.Width = new GridLength(300);
        }

        IPreferences.Current.UpdatePreference(PreferencesConstants.NewsPanelCollapsed, newValue);
    }

    private void RecentlyOpened_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RecentlyOpenedEmpty = FileViewModel.RecentlyOpened.Count == 0;
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void OpenFile(object parameter)
    {
        Application.Current.MainWindow.Activate();
        Close();
        FileViewModel.OpenFromOpenFileDialog();
    }

    private void OpenNewFile(object parameter)
    {
        Application.Current.MainWindow.Activate();
        Close();
        FileViewModel.CreateFromNewFileDialog();
    }

    private void OpenRecent(object parameter)
    {
        Application.Current.MainWindow.Activate();
        Close();
        FileViewModel.OpenRecent(parameter);
    }

    private void OpenInExplorer(object parameter)
    {
        if (parameter is not string value) 
            return;

        try
        {
            ProcessHelper.OpenInExplorer(value);
        }
        catch (RecoverableException e)
        {
            NoticeDialog.Show(e.DisplayMessage, "INTERNAL_ERROR");
        }
    }

    private bool CanOpenInExplorer(object parameter) => File.Exists((string)parameter);

    private async void HelloTherePopup_OnLoaded(object sender, RoutedEventArgs e)
    {
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
