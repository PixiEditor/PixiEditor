using PixiEditor.Parser;
using PixiEditor.SDK.FileParsers;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace PixiEditor.ExtensionExample
{
    [FileParser(".nopixi")]
    [Description("A example file type called .noPixi.\n Can only save layer images, name and is heavier than a regular .pixi file so you shouldn't actually use it")]
    public class ExampleDocumentParser : DocumentParser
    {
        public override bool UseBigEndian { get; } = true;

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override SerializableDocument Parse()
        {
            int width = ReadInt32();
            int height = ReadInt32();

            SerializableDocument document = new SerializableDocument(width, height);

            for (int i = 0; true; i++)
            {
                try
                {
                    int layerWidth = ReadInt32();
                    int layerHeight = ReadInt32();

                    int offsetX = ReadInt32();
                    int offsetY = ReadInt32();

                    string layerName = ReadString();

                    SerializableLayer layer = new SerializableLayer()
                    {
                        Width = layerWidth,
                        Height = layerHeight,
                        OffsetX = offsetX,
                        OffsetY = offsetY,
                        Name = layerName,
                        BitmapBytes = ReadBytes(ReadInt32())
                    };

                    document.Layers.Add(layer);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            return document;
        }

        public override void Save(SerializableDocument document)
        {
            WriteInt32(document.Width);
            WriteInt32(document.Height);

            foreach (SerializableLayer layer in document)
            {
                WriteInt32(layer.Width);
                WriteInt32(layer.Height);

                WriteInt32(layer.OffsetX);
                WriteInt32(layer.OffsetY);

                WriteString(layer.Name, true);

                WriteInt32(layer.BitmapBytes.Length);
                WriteBytes(layer.BitmapBytes);
            }
        }
    }
}
