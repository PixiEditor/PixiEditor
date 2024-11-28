using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.Models.Dialogs;

internal abstract class CustomDialog : ObservableObject
{
    protected Window OwnerWindow { get; }

    public CustomDialog(Window ownerWindow)
    {
        this.OwnerWindow = ownerWindow;
    }
    public abstract Task<bool> ShowDialog();
}
