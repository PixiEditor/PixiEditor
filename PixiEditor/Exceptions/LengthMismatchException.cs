using System;

namespace PixiEditor.Exceptions
{
    public class ArrayLengthMismatchException : Exception
    {
        public const string DefaultMessage = "First array length doesn't match second array length";

        public ArrayLengthMismatchException() : base(DefaultMessage)
        {
        }

        public ArrayLengthMismatchException(string message) : base(message)
        {
        }
    }
}