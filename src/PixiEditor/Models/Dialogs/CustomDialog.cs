using PixiEditor.Helpers;

namespace PixiEditor.Models.Dialogs;

internal abstract class CustomDialog : NotifyableObject
{
    public abstract bool ShowDialog();
}
