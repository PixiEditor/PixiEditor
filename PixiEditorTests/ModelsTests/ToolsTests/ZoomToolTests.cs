using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class ZoomToolTests
    {
        [StaFact]
        public void TestThatZoomSetsViewModelsZoomPercentage()
        {
            var vm = new ViewModelMain();
            var zoomTool = new ZoomTool();
            double zoom = 110;
            zoomTool.Zoom(zoom);
            Assert.Equal(zoom, vm.ZoomPercentage);
        }
    }
}