using System.IO;

namespace PixiEditor.Exceptions;

[Serializable]
internal class CorruptedFileException : RecoverableException
{
    public CorruptedFileException()
        : base("The file you've chosen might be corrupted.")
    {
    }

    public CorruptedFileException(string message)
        : base(message)
    {
    }

    public CorruptedFileException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected CorruptedFileException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
}
