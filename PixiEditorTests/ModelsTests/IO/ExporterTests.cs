using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using PixiEditor.Models.IO;
using Xunit;

namespace PixiEditorTests.ModelsTests.IO
{
    public class ExporterTests
    {
        private const string FilePath = "test.file";

        [Fact]
        public void TestThatSaveAsPngSavesFile()
        {
            Exporter.SaveAsPng(FilePath, 10, 10, BitmapFactory.New(10,10));
            Assert.True(File.Exists(FilePath));

            File.Delete(FilePath);
        }
    }
}
