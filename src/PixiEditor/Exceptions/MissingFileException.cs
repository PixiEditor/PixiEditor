using System.Runtime.Serialization;

namespace PixiEditor.Exceptions;

internal class MissingFileException : RecoverableException
{
    public string FilePath { get; set; }

    public MissingFileException() { }

    public MissingFileException(string message) : base(message) { }

    public MissingFileException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }

    public MissingFileException(string message, Exception innerException) : base(message, innerException) { }

    protected MissingFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
