using System.Runtime.Serialization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Exceptions;

internal class InvalidFileTypeException : RecoverableException
{
    public InvalidFileTypeException() { }

    public InvalidFileTypeException(LocalizedString displayMessage) : base(displayMessage) { }

    public InvalidFileTypeException(LocalizedString displayMessage, Exception innerException) : base(displayMessage, innerException) { }

    public InvalidFileTypeException(LocalizedString displayMessage, string exceptionMessage) : base(displayMessage, exceptionMessage) { }

    public InvalidFileTypeException(LocalizedString displayMessage, string exceptionMessage, Exception innerException) : base(displayMessage, exceptionMessage, innerException) { }

    protected InvalidFileTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

}
