using System.IO;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using Xunit;

namespace PixiEditorTests.ModelsTests.IO
{
    public class BinarySerializationTests
    {
        private const string Path = "bstests.file";

        [Fact]
        public void TestThatWriteToBinaryFileCreatesFile()
        {
            var doc = new SerializableDocument(new Document(10, 10));
            BinarySerialization.WriteToBinaryFile(Path, doc);

            Assert.True(File.Exists(Path));

            File.Delete(Path);
        }

        [Fact]
        public void TestThatReadFromBinaryFileReadsCorrectly()
        {
            var document = new Document(10, 10);
            document.Layers.Add(new Layer("yeet"));
            document.Swatches.Add(Colors.Green);

            var doc = new SerializableDocument(document);
            BinarySerialization.WriteToBinaryFile(Path, doc);

            var file = BinarySerialization.ReadFromBinaryFile<SerializableDocument>(Path);

            Assert.Equal(doc.Layers, file.Layers);
            Assert.Equal(doc.Height, file.Height);
            Assert.Equal(doc.Width, file.Width);
            Assert.Equal(doc.Swatches, file.Swatches);
        }
    }
}