using System.Diagnostics;
using System.Runtime.Serialization;

namespace PixiEditor.Exceptions;

/// <summary>
/// Allows to create a exception with a stacktrace without the exception having to be thrown
/// </summary>
public class CustomStackTraceException : Exception
{
    private string? stackTrace;
    
    public CustomStackTraceException()
    {
    }

    protected CustomStackTraceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CustomStackTraceException(string message) : base(message)
    {
    }

    public CustomStackTraceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public void GenerateStackTrace()
    {
        stackTrace = new StackTrace(1, true).ToString();
    }

    public override string? StackTrace => stackTrace ?? base.StackTrace;
}
