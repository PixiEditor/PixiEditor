using PixiEditor.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationPopup.xaml
    /// </summary>
    public partial class ConfirmationPopup : Window
    {
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SetResultAndCloseCommand { get; set; }

        public ConfirmationPopup()
        {
            InitializeComponent();
            CancelCommand = new RelayCommand(Cancel);
            SetResultAndCloseCommand = new RelayCommand(SetResultAndClose);
            DataContext = this;
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

        public bool Result
        {
            get { return (bool)GetValue(SaveChangesProperty); }
            set { SetValue(SaveChangesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SaveChanges.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveChangesProperty =
            DependencyProperty.Register("SaveChanges", typeof(bool), typeof(ConfirmationPopup), new PropertyMetadata(true));



        public string Body
        {
            get { return (string)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Body.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(string), typeof(ConfirmationPopup), new PropertyMetadata(""));

    }
}
