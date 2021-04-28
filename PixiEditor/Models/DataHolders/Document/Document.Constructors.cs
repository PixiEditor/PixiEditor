using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        public Document(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(0, 0, width, height));
        }

        private Document()
        {
            SetRelayCommands();
            UndoManager = new UndoManager();
            XamlAccesibleViewModel = ViewModelMain.Current ?? null;
            GeneratePreviewLayer();
        }
    }
}