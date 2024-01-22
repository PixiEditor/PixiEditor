using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public static class PixiEditorApiLinkerExtensions
{
    public static void DefinePixiEditorApi(this Linker linker, WasmExtensionInstance instance)
    {
        linker.DefineFunction("env", "log_message",(int messageOffset) =>
        {
            string messageString = MemoryUtility.GetStringFromWasmMemory(messageOffset, instance.Instance.GetMemory("memory"));
            Console.WriteLine(messageString);
        });

        linker.DefineFunction("env", "create_popup_window",(int titleOffset, int bodyOffset) =>
        {
            string title = MemoryUtility.GetStringFromWasmMemory(titleOffset, instance.Instance.GetMemory("memory"));
            string body = MemoryUtility.GetStringFromWasmMemory(bodyOffset, instance.Instance.GetMemory("memory"));
            instance.Api.WindowProvider.CreatePopupWindow(title, body).ShowDialog();
        });
    }
}
