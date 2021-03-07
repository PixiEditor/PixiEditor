using System;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Main;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    [Collection("Application collection")]
    public class DocumentLayersTests
    {
        [Fact]
        public void TestThatToggleLayerDoesNotToggleLastLayer()
        {
            Document doc = new (5, 5);
            doc.AddNewLayer("layer");
            bool isActive = doc.Layers[^1].IsActive;
            doc.ToggleLayer(0);
            Assert.False(doc.Layers[^1].IsActive != isActive);
        }

        [Fact]
        public void TestThatToggleLayerTogglesLayer()
        {
            Document doc = new (5, 5);
            doc.AddNewLayer("layer");
            doc.AddNewLayer("layer 1");
            doc.Layers[0].IsActive = true;
            doc.Layers[^1].IsActive = true;

            doc.ToggleLayer(0);
            Assert.False(doc.Layers[0].IsActive);
            Assert.True(doc.Layers[1].IsActive);
        }

        [Fact]
        public void TestThatToggleLayerDoesNothingOnNonExistingIndex()
        {
            Document document = new Document(5, 5);
            document.AddNewLayer("test");
            document.ToggleLayer(1);
            document.ToggleLayer(-1);
            Assert.True(true);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(2, 0)]
        [InlineData(1, 1)]
        public void TestThatSelectLayersRangeSelectsRange(int startIndex, int endIndex)
        {
            Document document = new Document(5, 5);

            document.AddNewLayer("1");
            document.AddNewLayer("2");
            document.AddNewLayer("3");

            document.SetMainActiveLayer(startIndex);

            document.SelectLayersRange(endIndex);

            for (int i = 0; i < document.Layers.Count; i++)
            {
                Assert.Equal(
                    i >= Math.Min(startIndex, endIndex)
                    && i <= Math.Max(startIndex, endIndex), 
                    document.Layers[i].IsActive);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void TestThatDeselectAllExceptDeselectsAllExceptLayer(int index)
        {
            Document document = new Document(5, 5);

            document.AddNewLayer("1");
            document.AddNewLayer("2");
            document.AddNewLayer("3");

            document.SetMainActiveLayer(0);
            document.Layers[1].IsActive = true;
            document.Layers[2].IsActive = true;

            document.DeselectAllExcept(document.Layers[index]);

            foreach (var layer in document.Layers)
            {
                Assert.Equal(layer == document.Layers[index], layer.IsActive);
            }
        }

        [Fact]
        public void TestThatUpdateLayersColorMakesOnlyOneLayerMainColorAndOtherSecondary()
        {
            Document document = new Document(1, 1);

            document.AddNewLayer("1");
            document.AddNewLayer("2");
            document.AddNewLayer("3");

            document.SetMainActiveLayer(0);
            document.Layers[1].IsActive = true; // This makes layer selected, but not main
            document.Layers[2].IsActive = true;

            document.UpdateLayersColor();

            Assert.Equal(Document.MainSelectedLayerColor, document.Layers[0].LayerHighlightColor);
            Assert.Equal(Document.SecondarySelectedLayerColor, document.Layers[1].LayerHighlightColor);
            Assert.Equal(Document.SecondarySelectedLayerColor, document.Layers[2].LayerHighlightColor);
        }

        [Fact]
        public void TestThatUpdateLayersColorMakesLayerMainColorAndRestNonActiveReturnsTransparent()
        {
            Document document = new Document(1, 1);

            document.AddNewLayer("1");
            document.AddNewLayer("2");
            document.AddNewLayer("3");

            document.SetMainActiveLayer(1);

            document.UpdateLayersColor();

            string transparentHex = "#00000000";

            Assert.Equal(transparentHex, document.Layers[0].LayerHighlightColor);
            Assert.Equal(Document.MainSelectedLayerColor, document.Layers[1].LayerHighlightColor);
            Assert.Equal(transparentHex, document.Layers[2].LayerHighlightColor);
        }

        [Fact]
        public void TestThatSetNextSelectedLayerAsActiveSelectsFirstAvailableLayer()
        {
            Document document = new Document(1, 1);

            document.AddNewLayer("1");
            document.AddNewLayer("2");
            document.AddNewLayer("3");
            document.AddNewLayer("4");

            foreach (var layer in document.Layers)
            {
                layer.IsActive = true;
            }

            document.SetNextSelectedLayerAsActive(document.Layers[1].LayerGuid);

            Assert.Equal(0, document.ActiveLayerIndex);
        }
    }
}