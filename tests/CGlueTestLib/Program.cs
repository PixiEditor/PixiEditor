using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Utilities;

namespace CGlueTestLib;

public static class Program
{
    public static void Main()
    {
        PixiEditorApi api = new PixiEditorApi(); // to prevent linker from removing the assembly
    }
}
