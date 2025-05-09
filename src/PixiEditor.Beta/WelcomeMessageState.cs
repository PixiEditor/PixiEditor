using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Beta;

public class WelcomeMessageState : State
{
    private const string Body1 = @"
We are extremely excited to share this version with you, early testers. Before you jump in and test all the new things, we have a few things to note:

- This is a very early version of PixiEditor 2.0. Not every feature promised in the roadmap is
  implemented yet. 

- App is not production ready! Expect bugs, crashes, unfinished features, placeholders and other signs of development.
";

    private const string Body2 =
        "- Your feedback is the most important thing of this beta, please take a moment to report any issues and suggestions on PixiEditor Forum.";

    private const string Body3 = @"
- We are collecting anonymous usage data to fix bugs, crashes and performance issues. This data will help us to improve the app. During the beta 
there is no option to opt-out. No personal data is collected.

Click on below checkboxes that you understand what you are getting into and you are ready to test the app.

I understand that:
";

    private bool[] _checkboxes = new bool[4];

    public event Action OnContinue;

    public override LayoutElement BuildElement()
    {
        return new Layout(body:
            new Align(
                Alignment.TopCenter,
                new Column(
                    new Center(new Text("Welcome to the open beta of PixiEditor 2.0!", TextWrap.Wrap,
                        new TextStyle(fontSize: 24))),
                    new Text(Body1, TextWrap.Wrap, new TextStyle(fontSize: 16)),
                    new Hyperlink("https://forum.pixieditor.net", Body2,
                        textStyle: new TextStyle(fontSize: 16), textWrap: TextWrap.Wrap),
                    new Text(Body3, TextWrap.Wrap, new TextStyle(fontSize: 16)),
                    new CheckBox(
                        new Text("The app may be unstable, crash or freeze",
                            textStyle: new TextStyle(fontStyle: FontStyle.Italic, fontSize: 16)),
                        onCheckedChanged: (args) => CheckboxChanged(args.Sender as CheckBox, 0)),
                    new CheckBox(
                        new Text("I may encounter unfinished features and placeholders",
                            textStyle: new TextStyle(fontSize: 16, fontStyle: FontStyle.Italic)),
                        onCheckedChanged: (args) => CheckboxChanged(args.Sender as CheckBox, 1)),
                    new CheckBox(new Text("I may lose my work due to bugs",
                        textStyle: new TextStyle(fontSize: 16, fontStyle: FontStyle.Italic)),
                        onCheckedChanged: (args) => CheckboxChanged(args.Sender as CheckBox, 2)),
                    new CheckBox(
                        new Text("I will have a lot of fun testing the app",
                            textStyle: new TextStyle(fontSize: 16, fontStyle: FontStyle.Italic)),
                        onCheckedChanged: (args) => CheckboxChanged(args.Sender as CheckBox, 3)),
                    new Container(
                        margin: new Edges(0, 5, 0, 0),
                        width: AllCheckBoxesChecked() ? 100 : 200,
                        child:
                        AllCheckBoxesChecked()
                            ? new Button(new Text("Continue"), onClick: (args) => { OnContinue?.Invoke(); })
                            : new Text("Select All Checkboxes to continue")
                    )
                )
            )
        );
    }

    void CheckboxChanged(CheckBox checkBox, int index)
    {
        SetState(() =>
        {
            _checkboxes[index] = checkBox.IsChecked;
        });
    }

    private bool AllCheckBoxesChecked()
    {
        return _checkboxes.All(x => x);
    }
}
