using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    [Collection("Application collection")]
    public class DocumentLayersTests
    {
        [Fact]
        public void TestThatToggleLayerTogglesLayer()
        {
            Document doc = new Document(5, 5);
            doc.AddNewLayer("layer");
            bool isActive = doc.Layers[^1].IsActive;
            doc.ToggleLayer(0);
            Assert.True(doc.Layers[^1].IsActive != isActive);
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

            document.SetActiveLayer(startIndex);

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

            document.SetActiveLayer(0);
            document.Layers[1].IsActive = true;
            document.Layers[2].IsActive = true;

            document.DeselectAllExcept(document.Layers[index]);

            foreach (var layer in document.Layers)
            {
                Assert.Equal(layer == document.Layers[index], layer.IsActive);
            }
        }
    }
}