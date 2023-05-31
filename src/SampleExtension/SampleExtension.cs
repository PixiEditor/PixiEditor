using System.Reflection;
using System.Windows.Controls;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Palettes;

namespace SampleExtension;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        var popup = Api.WindowProvider.CreatePopupWindow("Hello World!", new TextBlock { Text = "Hello World!" });
        Api.PaletteProvider.RegisterDataSource(new TestPaletteDataSource());
        popup.ShowDialog();
    }
}
