namespace PixiEditor.Models.IO.PaletteParsers;

public class SavingNotSupportedException : Exception
{
    public SavingNotSupportedException(string message) : base(message)
    {
    }
}
