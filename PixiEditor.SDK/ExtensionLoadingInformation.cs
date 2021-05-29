using PixiEditor.SDK.FileParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.SDK
{
    public class ExtensionLoadingInformation
    {
        internal Extension Extension { get; }

        internal List<DocumentParserInfo> DocumentParsers { get; set; } = new List<DocumentParserInfo>();

        internal List<ImageParserInfo> ImageParsers { get; set; } = new List<ImageParserInfo>();

        internal ExtensionLoadingInformation(Extension extension)
        {
            Extension = extension;
        }

        public ExtensionLoadingInformation AddDocumentParser(Type documentParserType)
        {
            DocumentParserInfo parserInfo = new DocumentParserInfo(documentParserType);

            DocumentParsers.Add(parserInfo);

            foreach (string s in parserInfo.SupportedFileExtensions)
            {
                Extension.SupportedDocumentFileExtensions.Add(s);
            }

            return this;
        }

        public ExtensionLoadingInformation AddDocumentParser<TParser>()
            where TParser : DocumentParser => AddDocumentParser(typeof(TParser));

        public ExtensionLoadingInformation AddImageParser(Type documentParserType)
        {
            ImageParserInfo parserInfo = new ImageParserInfo(documentParserType);

            ImageParsers.Add(parserInfo);

            foreach (string s in parserInfo.SupportedFileExtensions)
            {
                Extension.SupportedImageFileExtensions.Add(s);
            }

            return this;
        }

        public ExtensionLoadingInformation AddImageParser<TParser>()
            where TParser : ImageParser => AddImageParser(typeof(TParser));
    }
}
