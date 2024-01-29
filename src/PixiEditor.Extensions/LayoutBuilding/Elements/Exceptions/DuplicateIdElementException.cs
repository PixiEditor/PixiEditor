namespace PixiEditor.Extensions.LayoutBuilding.Elements.Exceptions;

public class DuplicateIdElementException : Exception
{
    public DuplicateIdElementException(int id) : base($"Element with id {id} already exists")
    {
    }
}
