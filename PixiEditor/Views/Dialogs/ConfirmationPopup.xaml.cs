using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ConfirmationPopup.xaml
    /// </summary>
    public partial class ConfirmationPopup : Window
    {
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(bool), typeof(ConfirmationPopup),
                new PropertyMetadata(true));

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(nameof(Body), typeof(string), typeof(ConfirmationPopup), new PropertyMetadata(""));

        public ConfirmationPopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            CancelCommand = new RelayCommand(Cancel);
            SetResultAndCloseCommand = new RelayCommand(SetResultAndClose);
            DataContext = this;
        }

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SetResultAndCloseCommand { get; set; }

        public bool Result
        {
            get => (bool)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }


        public string Body
        {
            get => (string)GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        private void SetResultAndClose(object property)
        {
            bool result = (bool)property;
            Result = result;
            DialogResult = true;
            Close();
        }

        private void Cancel(object property)
        {
            DialogResult = false;
            Close();
        }
    }
}
