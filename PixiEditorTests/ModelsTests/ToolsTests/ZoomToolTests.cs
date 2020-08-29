using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class ZoomToolTests
    {
        [StaFact]
        public void TestThatZoomSetsViewModelsZoomPercentage()
        {
            ViewModelMain vm = new ViewModelMain();
            ZoomTool zoomTool = new ZoomTool();
            double zoom = 110;
            zoomTool.Zoom(zoom);
            Assert.Equal(zoom, vm.ZoomPercentage);
        }

    }
}
