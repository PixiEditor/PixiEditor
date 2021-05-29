using System;

namespace PixiEditor.SDK.FileParsers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FileParserAttribute : Attribute
    {
        public string[] FileExtensions { get; set; }

        public FileParserAttribute(params string[] fileExtensions)
        {
            FileExtensions = fileExtensions;
        }
    }
}
