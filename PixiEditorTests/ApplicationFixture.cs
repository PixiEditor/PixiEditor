using System.Diagnostics.CodeAnalysis;
using System.Windows;
using PixiEditor;

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
        }
    }
}