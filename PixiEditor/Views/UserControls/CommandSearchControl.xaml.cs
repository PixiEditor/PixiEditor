using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.DataHolders;
using System.Windows.Input;
using System.Windows.Threading;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for CommandSearchControl.xaml
    /// </summary>
    public partial class CommandSearchControl : UserControl
    {
        private readonly CommandSearchViewModel _viewModel;

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register(nameof(SearchTerm), typeof(string), typeof(CommandSearchControl), new(SearchTermPropertyChanged));

        public string SearchTerm
        {
            get => _viewModel.SearchTerm;
            set => _viewModel.SearchTerm = value;
        }

        public CommandSearchControl()
        {
            InitializeComponent();
            _viewModel = mainGrid.DataContext as CommandSearchViewModel;
            _viewModel.PropertyChanged += (s, e) =>
            {
                SetValue(SearchTermProperty, _viewModel.SearchTerm);
            };

            var descriptor = DependencyPropertyDescriptor.FromProperty(VisibilityProperty, typeof(UserControl));
            descriptor.AddValueChanged(this, (sender, e) =>
            {
                if (Visibility == Visibility.Visible)
                {
                    Action action = () =>
                    {
                        textBox.Focus();
                    };
                    Application.Current.MainWindow.Dispatcher.Invoke(DispatcherPriority.Background, action);
                }
            });

            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            _viewModel.SelectedResult = null;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Enter)
            {
                Visibility = Visibility.Collapsed;
                _viewModel.SelectedResult.Execute();
            }
            else if (e.Key == Key.Down || e.Key == Key.PageDown)
            {
                var newIndex = _viewModel.Results.IndexOf(_viewModel.SelectedResult) + 1;
                if (newIndex >= _viewModel.Results.Count)
                {
                    newIndex = 0;
                }

                _viewModel.SelectedResult = _viewModel.Results.IndexOrNext(x => x.CanExecute, newIndex);
            }
            else if (e.Key == Key.Up || e.Key == Key.PageUp)
            {
                var newIndex = _viewModel.Results.IndexOf(_viewModel.SelectedResult);
                if (newIndex == -1)
                {
                    newIndex = 0;
                }
                if (newIndex == 0)
                {
                    newIndex = _viewModel.Results.Count - 1;
                }
                else
                {
                    newIndex--;
                }

                _viewModel.SelectedResult = _viewModel.Results.IndexOrPrevious(x => x.CanExecute, newIndex);
            }
            else if (CommandController.Current.Commands["PixiEditor.Search.Toggle"].Shortcut
                == new KeyCombination(e.Key, Keyboard.Modifiers) || e.Key == Key.Escape)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
                Keyboard.ClearFocus();
            }
            else
            {
                e.Handled = false;
            }
        }

        private static void SearchTermPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var control = dp as CommandSearchControl;

            control._viewModel.SearchTerm = e.NewValue as string;
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            var searchResult = (sender as Button).DataContext as SearchResult;

            _viewModel.SelectedResult = searchResult;
        }
    }
}
