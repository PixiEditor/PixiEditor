using System;
using System.IO;
using System.Reflection;

namespace PixiEditor.SDK.FileParsers
{
    internal abstract class FileParserInfo<TParser, T>
        where TParser : FileParser<T>
    {
        public Type ParserType { get; }

        public bool Enabled { get; set; } = true;

        public string[] SupportedFileExtensions { get; set; }

        private ConstructorInfo Constructor { get; }

        public TParser Create(Stream stream)
        {
            TParser parser = (TParser)Constructor.Invoke(null);
            parser.Stream = stream;

            return parser;
        }

        public TParser Create(Stream stream, string path)
        {
            TParser parser = Create(stream);
            parser.FileInfo = new FileInfo(path);

            return parser;
        }

        public FileParserInfo(Type parserType)
        {
            FileParserAttribute fileParserAttribute;

            if ((fileParserAttribute = parserType.GetCustomAttribute<FileParserAttribute>()) is null)
            {
                throw new ParserException(parserType, $"'{parserType}' needs an {nameof(FileParserAttribute)}");
            }

            SupportedFileExtensions = fileParserAttribute.FileExtensions;
            Constructor = parserType.GetConstructor(Type.EmptyTypes);

            if (Constructor is null)
            {
                throw new ParserException(parserType, $"'{parserType}' needs an constructor that no parameters");
            }

            ParserType = parserType;
        }
    }
}
