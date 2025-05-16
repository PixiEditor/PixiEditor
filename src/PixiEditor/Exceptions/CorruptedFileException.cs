using System.Runtime.Serialization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Exceptions;

[Serializable]
internal class CorruptedFileException : RecoverableException
{
    public CorruptedFileException() : base("FAILED_TO_OPEN_FILE") { }

    public CorruptedFileException(Exception innerException) : base("FAILED_TO_OPEN_FILE", innerException) { }

    public CorruptedFileException(LocalizedString displayMessage) : base(displayMessage) { }

    public CorruptedFileException(LocalizedString displayMessage, Exception innerException) : base(displayMessage, innerException) { }

    public CorruptedFileException(LocalizedString displayMessage, string exceptionMessage) : base(displayMessage, exceptionMessage) { }

    public CorruptedFileException(LocalizedString displayMessage, string exceptionMessage, Exception innerException) : base(displayMessage, exceptionMessage, innerException) { }

    protected CorruptedFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
