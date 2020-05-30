using PixiEditor.Helpers;
using System.Windows;

namespace PixiEditor.ViewModels
{
    class FeedbackDialogViewModel : ViewModelBase
    {
        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand SendButtonCommand { get; set; }


        private string _mailFrom;

        public string MailFrom
        {
            get { return _mailFrom; }
            set { if (_mailFrom != value) { _mailFrom = value; RaisePropertyChanged("MailFrom"); } }
        }


        private string _emailBody;

        public string EmailBody
        {
            get { return _emailBody; }
            set { if (_emailBody != value) { _emailBody = value; RaisePropertyChanged("EmailBody"); } }
        }

        public FeedbackDialogViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            SendButtonCommand = new RelayCommand(Send, CanSend);
        }

        private void CloseWindow(object parameter)
        {
            ((Window)parameter).DialogResult = false;
            base.CloseButton(parameter);
        }

        private void Send(object parameter)
        {
            base.CloseButton(parameter);
        }

        private bool CanSend(object property)
        {
            return !string.IsNullOrWhiteSpace(MailFrom);
        }
    }
}
