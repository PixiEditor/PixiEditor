using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ConfirmationPopup.xaml
    /// </summary>
    public partial class ConfirmationPopup : Window
    {
        // Using a DependencyProperty as the backing store for SaveChanges.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveChangesProperty =
            DependencyProperty.Register("SaveChanges", typeof(bool), typeof(ConfirmationPopup),
                new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for Body.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(string), typeof(ConfirmationPopup), new PropertyMetadata(""));

        public ConfirmationPopup()
        {
            InitializeComponent();
            CancelCommand = new RelayCommand(Cancel);
            SetResultAndCloseCommand = new RelayCommand(SetResultAndClose);
            DataContext = this;
        }

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SetResultAndCloseCommand { get; set; }

        public bool Result
        {
            get => (bool) GetValue(SaveChangesProperty);
            set => SetValue(SaveChangesProperty, value);
        }


        public string Body
        {
            get => (string) GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        private void SetResultAndClose(object property)
        {
            var result = (bool) property;
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