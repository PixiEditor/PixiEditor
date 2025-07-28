namespace PixiEditor.Models.ExceptionHandling;

public class CrashInfoCollectionException(string collecting, Exception caught) : Exception($"Caught {caught.GetType().FullName} while collecting {collecting}.", caught)
{
}
