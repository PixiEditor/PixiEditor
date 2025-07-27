namespace PixiEditor.PixiAuth.Exceptions;

public class PixiAuthException : Exception
{
    public int StatusCode { get; }

    public PixiAuthException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
