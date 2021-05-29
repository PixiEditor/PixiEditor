using System;
using PixiEditor.Parser;

namespace PixiEditor.SDK.FileParsers
{
    internal class DocumentParserInfo : FileParserInfo<DocumentParser, SerializableDocument>
    {
        public DocumentParserInfo(Type documentParserType)
            : base(documentParserType)
        {
        }
    }
}
