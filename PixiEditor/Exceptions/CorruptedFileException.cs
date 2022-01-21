using System;

namespace PixiEditor.Exceptions
{
    [Serializable]
    public class CorruptedFileException : Exception
    {
        public CorruptedFileException()
            : base("The file you chose might be corrupted.")
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
}
