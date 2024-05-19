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

        var showTask = Api.WindowProvider.CreatePopupWindow("WASM SampleExtension", layout).ShowDialog();
        showTask.Completed += result =>
        {
            Api.Logger.Log($"Show task completed: {result}");
        };
    }
}

