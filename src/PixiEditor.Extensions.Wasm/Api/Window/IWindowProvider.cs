using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public interface IWindowProvider
{
    public void CreatePopupWindow(string title, NativeControl body);
}
