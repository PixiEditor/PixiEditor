using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.ClosedBeta;

public class WelcomeMessageState : State
{
    private const string Body = @"
We are extremely exicted to share this version to you, early testers. Before you jump in and test all the new things,
we have a few things to note:

- This is a very first publicly available version of PixiEditor 2.0. Not every feature promised in the roadmap is
  implemented yet. 
- App is not production ready! Expect bugs, crashes, unfinished features, placeholders and other signs of development.
- Your feedback is the most important thing of this beta, please take a moment to report any issues and suggestions on the Discord channel.
- Promised features available in this beta are: Animations, Procedural Art (Nodes)

Click on below checkboxes that you understand what you are getting into and you are ready to test the app.
";

    public override LayoutElement BuildElement()
    {
        return new Layout(body:
            new Align(
                Alignment.TopCenter,
                new Column(
                    new Center(new Text("Welcome to the closed beta of PixiEditor 2.0!", TextWrap.Wrap,
                        FontStyle.Normal,
                        fontSize: 24)),
                    new Text(Body, TextWrap.Wrap, fontSize: 18),
                    new Container(
                        width: 100,
                        child:
                        new Button(new Text("Continue")
                        )
                    )
                )
            )
        );
    }
}
