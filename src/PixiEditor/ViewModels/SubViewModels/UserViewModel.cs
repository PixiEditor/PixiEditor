using PixiEditor.PixiAuth;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UserViewModel : SubViewModel<ViewModelMain>
{
    public PixiAuthClient PixiAuthClient { get; }

    public UserViewModel(ViewModelMain owner) : base(owner)
    {
        string baseUrl = BuildConstants.PixiEditorApiUrl;
#if DEBUG
        if (baseUrl.Contains('{') && baseUrl.Contains('}'))
        {
            string? envUrl = Environment.GetEnvironmentVariable("PIXIEDITOR_API_URL");
            if (envUrl != null)
            {
                baseUrl = envUrl;
            }
        }
#endif
        PixiAuthClient = new PixiAuthClient(BuildConstants.PixiEditorApiUrl);
    }
}
