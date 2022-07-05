namespace PixiEditor.ViewModels.SubViewModels.Document;

internal class DocumentViewModel : SubViewModel<ViewModelMain>
{
    public const string ConfirmationDialogTitle = "Unsaved changes";
    public const string ConfirmationDialogMessage = "The document has been modified. Do you want to save changes?";

    public DocumentViewModel(ViewModelMain owner, string name)
        : base(owner)
    {
    }
}
