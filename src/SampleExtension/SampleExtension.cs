using PixiEditor.Extensions;

namespace SampleExtension;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        /*var popup = Api.WindowProvider.CreatePopupWindow("Hello World!", new TextBlock
        {
            Text = "Hello World!", Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        });
        Api.PaletteProvider.RegisterDataSource(new TestPaletteDataSource());
        popup.ShowDialog();*/
    }
}
