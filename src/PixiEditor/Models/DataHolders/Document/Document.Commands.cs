using PixiEditor.Helpers;

namespace PixiEditor.Models.DataHolders.Document;

internal partial class Document
{
    public RelayCommand RequestCloseDocumentCommand { get; set; }

    public RelayCommand SetAsActiveOnClickCommand { get; set; }

    private void SetRelayCommands()
    {
        //RequestCloseDocumentCommand = new RelayCommand(RequestCloseDocument);
        //SetAsActiveOnClickCommand = new RelayCommand(SetAsActiveOnClick);
    }
}
