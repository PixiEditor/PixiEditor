using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.Extensions;
using PixiEditor.Extensions.CommonApi.Windowing;

namespace SampleExtension;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        var popup = Api.Windowing.CreatePopupWindow("Hello World!", new TextBlock
        {
            Text = "Hello World!", Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        });
        Api.PaletteProvider.RegisterDataSource(new TestPaletteDataSource());
        popup.ShowDialog();

        Api.Windowing.GetWindow(WindowType.PalettesBrowser).Show();
    }
}
