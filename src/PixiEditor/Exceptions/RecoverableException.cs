using System.Runtime.Serialization;

namespace PixiEditor.Exceptions;

public class RecoverableException : Exception
{
    public RecoverableException() { }

    public RecoverableException(string message) : base(message) { }

    public RecoverableException(string message, Exception innerException) : base(message, innerException) { }

    protected RecoverableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
