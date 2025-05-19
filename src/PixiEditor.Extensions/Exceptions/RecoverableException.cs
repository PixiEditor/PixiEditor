using System.Runtime.Serialization;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Extensions.Exceptions;

public class RecoverableException : Exception
{
    public LocalizedString DisplayMessage { get; set; }

    public RecoverableException() 
    {
        DisplayMessage = "INTERNAL_ERROR";
    }

    public RecoverableException(LocalizedString displayMessage) 
    {
        DisplayMessage = displayMessage;
    }

    public RecoverableException(LocalizedString displayMessage, Exception innerException) : base(null, innerException) 
    {
        DisplayMessage = displayMessage;
    }

    public RecoverableException(LocalizedString displayMessage, string exceptionMessage) : base(exceptionMessage)
    {
        DisplayMessage = displayMessage;
    }

    public RecoverableException(LocalizedString displayMessage, string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
    {
        DisplayMessage = displayMessage;
    }

    protected RecoverableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
