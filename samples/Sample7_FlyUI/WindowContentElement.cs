using System.Diagnostics.CodeAnalysis;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Api.Window;

namespace FlyUISample;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "FlyUI style")]
public class WindowContentElement : StatelessElement
{
    public PopupWindow Window { get; set; }

    public override ControlDefinition BuildNative()
    {
        Layout layout = new Layout(body:
            new Container(margin: Edges.All(25), child:
                new Column(
                    crossAxisAlignment: CrossAxisAlignment.Center,
                    mainAxisAlignment: MainAxisAlignment.SpaceEvenly,
                    children:
                    [
                        new Center(
                            new Text(
                                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vitae neque nibh. Duis sed pharetra dolor. Donec dui sapien, aliquam id sodales in, ornare et urna. Mauris nunc odio, sagittis eget lectus at, imperdiet ornare quam. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod pellentesque blandit. Vestibulum sagittis, ligula non finibus lobortis, dolor lacus consectetur turpis, id facilisis ligula dolor vitae augue.",
                                wrap: TextWrap.Wrap,
                                textStyle: new TextStyle(fontSize: 16))
                        ),
                        new Align(
                            alignment: Alignment.CenterRight,
                            child: new Text("- Paulo Coelho, The Alchemist (1233)", textStyle: new TextStyle(fontStyle: FontStyle.Italic))
                        ),
                        new Container(
                            margin: Edges.Symmetric(25, 0),
                            backgroundColor: Color.FromRgba(25, 25, 25, 255),
                            child: new Column(
                                new Image(
                                    "/Pizza.png",
                                    filterQuality: FilterQuality.None,
                                    width: 256, height: 256))
                        ),
                        new CheckBox(new Text("heloo"),
                            onCheckedChanged: args =>
                            {
                                PixiEditorExtension.Api.Logger.Log(((CheckBox)args.Sender).IsChecked
                                    ? "Checked"
                                    : "Unchecked");
                            }),
                        new Center(
                            new Button(
                                child: new Text("Close"), onClick: _ => { Window.Close(); }))
                    ]
                )
            )
        );

        return layout.BuildNative();
    }

}