namespace PixiEditor.Extensions.FlyUI.Exceptions;

public class DuplicateIdElementException : Exception
{
    public DuplicateIdElementException(int id) : base($"Element with id {id} already exists")
    {
    }
}
