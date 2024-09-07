namespace PixiEditor.Exceptions;

public class CommandInvocationException : Exception
{
    public CommandInvocationException(string commandName, Exception? innerException = null) : 
        base($"Command '{commandName}' threw an exception", innerException) { }
}
