using System.Runtime.Serialization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Exceptions;

internal class MissingFileException : RecoverableException
{
    public MissingFileException() : base("FILE_NOT_FOUND") { }

    public MissingFileException(Exception innerException) : base("FILE_NOT_FOUND", innerException) { }

    public MissingFileException(LocalizedString displayMessage) : base(displayMessage) { }

    public MissingFileException(LocalizedString displayMessage, Exception innerException) : base(displayMessage, innerException) { }

    public MissingFileException(LocalizedString displayMessage, string exceptionMessage) : base(displayMessage, exceptionMessage) { }

    public MissingFileException(LocalizedString displayMessage, string exceptionMessage, Exception innerException) : base(displayMessage, exceptionMessage, innerException) { }

    protected MissingFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
