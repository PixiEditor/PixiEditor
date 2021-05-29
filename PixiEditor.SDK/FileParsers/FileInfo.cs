using System.IO;

namespace PixiEditor.SDK.FileParsers
{
    public class FileInfo
    {
        /// <summary>
        /// Gets the file name without the file extension
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the file extension of the file, including the dot
        /// </summary>
        public string FileExtension { get; private set; }

        internal FileInfo(string path)
        {
            FileName = Path.GetFileNameWithoutExtension(path);
            FileExtension = Path.GetExtension(path);
        }

        internal FileInfo(string name, string extension)
        {
            FileName = name;
            FileExtension = extension;
        }
    }
}
