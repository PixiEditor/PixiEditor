using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using PixiEditor;
using PixiEditor.Models.Undo;

namespace PixiEditorTests
{
    [ExcludeFromCodeCoverage]
    public class ApplicationFixture
    {
        public ApplicationFixture()
        {
            if (Application.Current == null)
            {
                App app = new App();
                app.InitializeComponent();
            }

            if (!Directory.Exists(Path.GetDirectoryName(StorageBasedChange.DefaultUndoChangeLocation)))
            {
                Directory.CreateDirectory(StorageBasedChange.DefaultUndoChangeLocation);
            }
        }
    }
}