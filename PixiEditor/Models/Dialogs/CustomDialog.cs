using ReactiveUI;

namespace PixiEditor.Models.Dialogs
{
    public abstract class CustomDialog : ReactiveObject
    {
        public abstract bool ShowDialog();
    }
}