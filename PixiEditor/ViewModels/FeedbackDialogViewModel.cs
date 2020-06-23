using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels
{
    internal class FeedbackDialogViewModel : ViewModelBase
    {
        private string _emailBody;


        private string _mailFrom;

        public FeedbackDialogViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseWindow);
            SendButtonCommand = new RelayCommand(Send, CanSend);
        }

        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand SendButtonCommand { get; set; }

        public string MailFrom
        {
            get => _mailFrom;
            set
            {
                if (_mailFrom != value)
                {
                    _mailFrom = value;
                    RaisePropertyChanged("MailFrom");
                }
            }
        }

        public string EmailBody
        {
            get => _emailBody;
            set
            {
                if (_emailBody != value)
                {
                    _emailBody = value;
                    RaisePropertyChanged("EmailBody");
                }
            }
        }

        private void CloseWindow(object parameter)
        {
            ((Window) parameter).DialogResult = false;
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