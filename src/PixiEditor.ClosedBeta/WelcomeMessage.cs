using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.ClosedBeta;

public class WelcomeMessage : StatefulElement<WelcomeMessageState>
{
    public override WelcomeMessageState CreateState()
    {
        return new WelcomeMessageState();
    }
}
