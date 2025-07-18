namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

public class EventsModule : ApiModule
{
    public EventsModule(WasmExtensionInstance extension) : base(extension)
    {
        extension.Api.UserDataProvider.UserLoggedIn += OnUserLoggedIn;
        extension.Api.UserDataProvider.UserLoggedOut += OnUserLoggedOut;
    }

    private void OnUserLoggedIn()
    {
        Extension.Instance?.GetAction("on_user_logged_in")?.Invoke();
    }

    private void OnUserLoggedOut()
    {
        Extension.Instance?.GetAction("on_user_logged_out")?.Invoke();
    }
}
