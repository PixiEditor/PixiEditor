using System.Runtime.Serialization;
using PixiEditor.Localization;

namespace PixiEditor.Exceptions;

internal class MissingFileException : RecoverableException
{
    public string FilePath { get; set; }

    public MissingFileException() { }

    public MissingFileException(string message) : base(message) { }

    public MissingFileException(string message, Exception innerException) : base(message, innerException) { }

    public MissingFileException(string message, LocalizedString displayMessage) : base(message, displayMessage) { }

    public MissingFileException(string message, LocalizedString displayMessage, Exception innerException) : base(message, displayMessage, innerException) { }

    protected MissingFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public MissingFileException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }

    public MissingFileException(string message, LocalizedString displayMessage, string filePath) : base(message, displayMessage)
    {
        FilePath = filePath;
    }
}
