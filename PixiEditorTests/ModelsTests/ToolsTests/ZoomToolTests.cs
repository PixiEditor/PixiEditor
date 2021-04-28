using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class ZoomToolTests
    {
        [StaFact]
        public void TestThatZoomSetsActiveDocumentZoomPercentage()
        {
            ViewModelMain vm = new ViewModelMain(new ServiceCollection().AddSingleton<IPreferences>(new Mocks.PreferenceSettingsMock()).BuildServiceProvider());
            vm.BitmapManager.ActiveDocument = new PixiEditor.Models.DataHolders.Document(10, 10);
            ZoomTool zoomTool = new ZoomTool();
            double zoom = 110;
            zoomTool.Zoom(zoom);
            Assert.Equal(zoom, vm.BitmapManager.ActiveDocument.ZoomPercentage);
        }
    }
}