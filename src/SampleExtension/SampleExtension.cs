using PixiEditor.Extensions;

namespace SampleExtension;

public class SampleExtension : Extension
{
    protected override void OnLoaded()
    {
    }

    protected override void OnInitialized()
    {
        NoticeDialog($"Hello from {Metadata.DisplayName}", "SampleExtension");
    }
}
