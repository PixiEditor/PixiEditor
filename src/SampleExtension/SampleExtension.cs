using PixiEditor.Extensions;

namespace SampleExtension;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        var popup = Api.WindowProvider.CreatePopupWindow("Hello World!",
            "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">Hello World!</TextBlock>");
        popup.ShowDialog();
    }
}
