using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.Beta;

public class WelcomeMessage : StatefulElement<WelcomeMessageState>
{
    public event Action OnContinue;
    
    public override WelcomeMessageState CreateState()
    { 
        WelcomeMessageState state = new WelcomeMessageState();
        state.OnContinue += () => OnContinue?.Invoke();
        return state;
    }
}
