using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class FeedbackDialogViewModel : ViewModelBase
    {
        private string emailBody;

        private string mailFrom;

        public FeedbackDialogViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            SendButtonCommand = new RelayCommand(Send, CanSend);
        }

        public RelayCommand CloseButtonCommand { get; set; }

        public RelayCommand SendButtonCommand { get; set; }

        public string MailFrom
        {
            get => mailFrom;
            set
            {
                if (mailFrom != value)
                {
                    mailFrom = value;
                    RaisePropertyChanged("MailFrom");
                }
            }
        }

        public string EmailBody
        {
            get => emailBody;
            set
            {
                if (emailBody != value)
                {
                    emailBody = value;
                    RaisePropertyChanged("EmailBody");
                }
            }
        }

        private void CloseWindow(object parameter)
        {
            ((Window)parameter).DialogResult = false;
            CloseButton(parameter);
        }

        private void Send(object parameter)
        {
            CloseButton(parameter);
        }

        private bool CanSend(object property)
        {
            return !string.IsNullOrWhiteSpace(MailFrom);
        }
    }
}