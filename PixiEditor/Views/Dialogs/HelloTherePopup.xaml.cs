using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for HelloTherePopup.xaml.
    /// </summary>
    public partial class HelloTherePopup : Window
    {
        public RecentlyOpenedCollection RecentlyOpened { get => FileViewModel.RecentlyOpened; }

        public static readonly DependencyProperty FileViewModelProperty =
            DependencyProperty.Register(nameof(FileViewModel), typeof(FileViewModel), typeof(HelloTherePopup));

        public FileViewModel FileViewModel { get => (FileViewModel)GetValue(FileViewModelProperty); set => SetValue(FileViewModelProperty, value); }

        public static readonly DependencyProperty RecentlyOpenedEmptyProperty =
            DependencyProperty.Register(nameof(RecentlyOpenedEmpty), typeof(bool), typeof(HelloTherePopup));

        public bool RecentlyOpenedEmpty { get => (bool)GetValue(RecentlyOpenedEmptyProperty); set => SetValue(RecentlyOpenedEmptyProperty, value); }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand OpenNewFileCommand { get; set; }

        public RelayCommand OpenRecentCommand { get; set; }

        public RelayCommand OpenHyperlinkCommand { get => FileViewModel.Owner.MiscSubViewModel.OpenHyperlinkCommand; }

        public HelloTherePopup(FileViewModel fileViewModel)
        {
            DataContext = this;
            FileViewModel = fileViewModel;

            OpenFileCommand = new RelayCommand(OpenFile);
            OpenNewFileCommand = new RelayCommand(OpenNewFile);
            OpenRecentCommand = new RelayCommand(OpenRecent);

            RecentlyOpenedEmpty = RecentlyOpened.Count == 0;
            RecentlyOpened.CollectionChanged += RecentlyOpened_CollectionChanged;

            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            CloseIfRelease();
        }

        private void RecentlyOpened_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RecentlyOpenedEmpty = FileViewModel.RecentlyOpened.Count == 0;
        }

        [Conditional("RELEASE")]
        private void CloseIfRelease()
        {
            Close();
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
            Close();
            FileViewModel.OpenAny();
        }

        private void OpenNewFile(object parameter)
        {
            Close();
            FileViewModel.OpenNewFilePopup(parameter);
        }

        private void OpenRecent(object parameter)
        {
            FileViewModel.OpenRecent(parameter);
            Close();
        }
    }
}