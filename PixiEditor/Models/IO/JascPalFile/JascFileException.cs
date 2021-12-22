using System;

namespace PixiEditor.Models.IO.JascPalFile;

public class JascFileException : Exception
{
    public JascFileException(string message) : base(message)
    {
    }
}