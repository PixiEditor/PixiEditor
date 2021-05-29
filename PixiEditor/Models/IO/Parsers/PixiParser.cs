using PixiEditor.Parser;
using PixiEditor.SDK.FileParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.IO.Parsers
{
    public class PixiParser : DocumentParser
    {
        public override bool UseBigEndian => true;

        public override SerializableDocument Parse() => Parser.PixiParser.Deserialize(Stream);

        public override void Save(SerializableDocument value) => Parser.PixiParser.Serialize(value);
    }
}
