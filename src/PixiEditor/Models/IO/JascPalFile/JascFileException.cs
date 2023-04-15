using PixiEditor.Exceptions;

namespace PixiEditor.Models.IO.JascPalFile;

internal class JascFileException : RecoverableException
{
    public JascFileException(string message) : base(message)
    {
    }
}
