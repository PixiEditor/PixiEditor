using System.Threading.Tasks;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace CreatePopupSample;

public class CreatePopupSampleExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is loaded.
    ///  All extensions are first loaded and then initialized. This method is called before <see cref="OnInitialized"/>.
    /// </summary>
    public override void OnLoaded()
    {

    }

    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        var popup = Api.WindowProvider.CreatePopupWindow("Hello World", new Text("Hello from popup!"));
        popup.ShowDialog().Completed += (result) =>
        {
            string resultStr = result.HasValue ? result.Value.ToString() : "null";
            Api.Logger.Log($"Popup closed with result: {resultStr}");
        };
    }
}