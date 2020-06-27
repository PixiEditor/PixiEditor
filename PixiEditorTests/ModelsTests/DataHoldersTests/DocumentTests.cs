using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class DocumentTests
    {

        [Theory]
        [InlineData(10,10,20,20)]
        [InlineData(1,2,5,8)]
        [InlineData(20,20,10,10)] //TODO Anchor
        public void TestResizeCanvasResizesProperly(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            Document document = new Document(oldWidth, oldHeight);

            document.ResizeCanvas(newWidth, newHeight, AnchorPoint.Top | AnchorPoint.Left);
            Assert.Equal(newHeight, document.Height);
            Assert.Equal(newWidth, document.Width);
        }

        [Theory]
        [InlineData(10,10,20,20)]
        [InlineData(5,8,10,16)]
        public void TestResizeWorks(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            Document document = new Document(oldWidth, oldHeight);

            document.Resize(newWidth, newHeight);

            Assert.Equal(newHeight, document.Height);
            Assert.Equal(newWidth, document.Width);
        }

        [Theory]
        [InlineData(10,10, 0, 0)]
        [InlineData(50,50, 10, 49)]
        public void TestThatClipCanvasWorksForSingleLayer(int initialWidth, int initialHeight,int additionalPixelX, int additionalPixelY)
        {
            Document document = new Document(initialWidth, initialHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = document
            };
            manager.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(new Coordinates((int)Math.Ceiling(initialWidth / 2f), 
                (int)Math.Ceiling(initialHeight / 2f)), Colors.Black);

            manager.ActiveLayer.SetPixel(new Coordinates(additionalPixelX, additionalPixelY), Colors.Black);

            document.ClipCanvas();
            
            Assert.Equal(manager.ActiveLayer.Width, document.Width);
            Assert.Equal(manager.ActiveLayer.Height, document.Height);
        }

        [Theory]
        [InlineData(10, 10, 0, 0)]
        [InlineData(50, 50, 15, 23)]
        [InlineData(3, 3, 1, 1)]
        [InlineData(1, 1, 0, 0)]
        public void TestThatClipCanvasWorksForMultipleLayers(int initialWidth, int initialHeight, int secondLayerPixelX, int secondLayerPixelY)
        {
            Document document = new Document(initialWidth, initialHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = document
            };
            manager.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(new Coordinates((int)Math.Ceiling(initialWidth / 2f),
                (int)Math.Ceiling(initialHeight / 2f)), Colors.Black); //Set pixel in center

            manager.AddNewLayer("test2");

            manager.ActiveLayer.SetPixel(new Coordinates(secondLayerPixelX, secondLayerPixelY), Colors.Black);

            document.ClipCanvas();

            int totalWidth = Math.Abs(manager.ActiveDocument.Layers[1].OffsetX +
                             manager.ActiveDocument.Layers[1].Width - (manager.ActiveDocument.Layers[0].OffsetX +
                             manager.ActiveDocument.Layers[0].Width)) + 1;

            int totalHeight = Math.Abs(manager.ActiveDocument.Layers[1].OffsetY +
                manager.ActiveDocument.Layers[1].Height - (manager.ActiveDocument.Layers[0].OffsetY +
                                                          manager.ActiveDocument.Layers[0].Height)) + 1;

            Assert.Equal(totalWidth, document.Width);
            Assert.Equal(totalHeight, document.Height);
        }

        [Theory]
        [InlineData(10,10)]
        [InlineData(11,11)]
        [InlineData(25,17)]
        public void TestThatCenterContentCentersContentForSingleLayer(int docWidth, int docHeight)
        {
            Document doc = new Document(docWidth, docHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = doc
            };
            manager.AddNewLayer("test");

            manager.ActiveLayer.SetPixel(new Coordinates(0,0), Colors.Green);

            doc.CenterContent();

            Assert.Equal(Math.Floor(docWidth / 2f), manager.ActiveLayer.OffsetX);
            Assert.Equal(Math.Floor(docHeight / 2f), manager.ActiveLayer.OffsetY);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(11, 11)]
        [InlineData(25, 17)]
        public void TestThatCenterContentCentersContentForMultipleLayers(int docWidth, int docHeight)
        {
            Document doc = new Document(docWidth, docHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = doc
            };
            manager.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(new Coordinates(0, 0), Colors.Green);

            manager.AddNewLayer("test2");
            manager.ActiveLayer.SetPixel(new Coordinates(1, 1), Colors.Green);

            doc.CenterContent();

            int midWidth = (int)Math.Floor(docWidth / 2f);
            int midHeight = (int)Math.Floor(docHeight / 2f);

            Assert.Equal( midWidth - 1, manager.ActiveDocument.Layers[0].OffsetX);
            Assert.Equal( midHeight - 1, manager.ActiveDocument.Layers[0].OffsetY);

            Assert.Equal(midWidth, manager.ActiveDocument.Layers[1].OffsetX);
            Assert.Equal(midHeight, manager.ActiveDocument.Layers[1].OffsetY);
        }

    }
}
