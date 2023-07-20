namespace PixiEditor.Avalonia.Exceptions.Exceptions;

public class DependencyNotFoundException : Exception
{
    public DependencyNotFoundException(Type dependencyType) : base($"Dependency of type {dependencyType} not found.") { }

    public DependencyNotFoundException(string message) : base(message) { }

    public DependencyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
