using System.Runtime.Serialization;
using PixiEditor.Localization;

namespace PixiEditor.Exceptions;

public class RecoverableException : Exception
{
    public LocalizedString DisplayMessage { get; set; }

    public RecoverableException() { }

    public RecoverableException(string message) : base(message) { }

    public RecoverableException(string message, Exception innerException) : base(message, innerException) { }

    protected RecoverableException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public RecoverableException(string message, LocalizedString displayMessage) : base(message)
    {
        DisplayMessage = displayMessage;
    }

    public RecoverableException(string message, LocalizedString displayMessage, Exception innerException) : base(message, innerException)
    {
        DisplayMessage = displayMessage;
    }
}
