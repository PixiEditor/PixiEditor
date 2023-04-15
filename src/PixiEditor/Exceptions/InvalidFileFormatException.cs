using System.Runtime.Serialization;

namespace PixiEditor.Exceptions;

internal class InvalidFileFormatException : RecoverableException
{
    public string FilePath { get; set; }

    public InvalidFileFormatException() { }

    public InvalidFileFormatException(string message) : base(message) { }

    public InvalidFileFormatException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }

    public InvalidFileFormatException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
