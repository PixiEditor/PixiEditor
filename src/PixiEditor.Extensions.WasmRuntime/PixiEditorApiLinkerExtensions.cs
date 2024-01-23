using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public static class PixiEditorApiLinkerExtensions
{
    public static void DefinePixiEditorApi(this Linker linker, WasmExtensionInstance instance)
    {
        linker.DefineFunction("env", "log_message",(int messageOffset, int messageLength) =>
        {
            string messageString = MemoryUtility.GetStringFromWasmMemory(messageOffset, messageLength, instance.Instance.GetMemory("memory"));
            Console.WriteLine(messageString.ReplaceLineEndings());
        });

        linker.DefineFunction("env", "create_popup_window",(int titleOffset, int titleLength, int bodyOffset, int bodyLength) =>
        {
            string title = MemoryUtility.GetStringFromWasmMemory(titleOffset, titleLength, instance.Instance.GetMemory("memory"));
            string body = MemoryUtility.GetStringFromWasmMemory(bodyOffset, bodyLength, instance.Instance.GetMemory("memory"));
            instance.Api.WindowProvider.CreatePopupWindow(title, body).ShowDialog();
        });
    }
}
