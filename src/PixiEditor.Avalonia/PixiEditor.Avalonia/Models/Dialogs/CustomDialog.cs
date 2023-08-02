using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.Models.Dialogs;

internal abstract class CustomDialog : ObservableObject
{
    public abstract Task<bool> ShowDialog();
}
