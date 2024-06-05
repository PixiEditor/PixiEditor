using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Wasm.Api.FlyUI;
using PixiEditor.Extensions.Wasm.Api.Localization;

namespace FlyUISample;

public class WindowContentElement : StatelessElement
{
    public override CompiledControl BuildNative()
    {
        Layout layout = new Layout(body:
            new Container(margin: Edges.All(25), child:
                new Column(
                    new Center(
                        new Text(
                            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed vitae neque nibh. Duis sed pharetra dolor. Donec dui sapien, aliquam id sodales in, ornare et urna. Mauris nunc odio, sagittis eget lectus at, imperdiet ornare quam. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod pellentesque blandit. Vestibulum sagittis, ligula non finibus lobortis, dolor lacus consectetur turpis, id facilisis ligula dolor vitae augue.",
                            wrap: TextWrap.Wrap)
                    ),
                    new Align(
                        alignment: Alignment.CenterRight,
                        child: new Text("- Paulo Coelho, The Alchemist (1233)", fontStyle: FontStyle.Italic)
                    ),
                    new Container(
                        margin: Edges.Symmetric(25, 0), 
                        backgroundColor: Color.FromRgba(25, 25, 25, 255),
                        child: new Column(
                            new Image("/Pizza.png", filterQuality: FilterQuality.None)))
                )
            )
        );

        return layout.BuildNative();
    }
}