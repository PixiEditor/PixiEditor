using PixiEditor.Parser;
using PixiEditor.SDK.FileParsers;

namespace PixiEditor.Models.IO.Parsers
{
    [FileParser(".pixi")]
    public class PixiParser : DocumentParser
    {
        public override bool UseBigEndian => true;

        public override SerializableDocument Parse() => Parser.PixiParser.Deserialize(Stream);

        public override void Save(SerializableDocument value) => Parser.PixiParser.Serialize(value);
    }
}
