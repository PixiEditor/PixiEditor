using System;
using PixiEditor.Extensions.Wasm;
using PixiEditor.Extensions.Wasm.Api.FlyUI;

namespace WasmSampleExtension;

public class SampleExtension : WasmExtension
{
    public override void OnLoaded()
    {
        Api.Logger.Log("WASM SampleExtension loaded!");
    }

    public override void OnInitialized()
    {
        Api.Logger.Log("WASM SampleExtension initialized!");

        Layout layout = new Layout(
            new Center(
                child: new ButtonTextElement()
                )
            );

        var window = Api.WindowProvider.CreatePopupWindow("WASM SampleExtension", layout);
        window.Width = 200;
        window.Height = 200;
        window.CanResize = false;
        window.CanMinimize = false;
        var showTask = window.ShowDialog();
        showTask.Completed += result =>
        {
            Api.Logger.Log($"Show task completed: {result}");
        };
    }
}

