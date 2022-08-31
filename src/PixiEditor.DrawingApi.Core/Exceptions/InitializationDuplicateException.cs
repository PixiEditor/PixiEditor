using System;

namespace PixiEditor.DrawingApi.Core.Exceptions
{
    public class InitializationDuplicateException : Exception
    {
        public InitializationDuplicateException()
        {
        }

        public InitializationDuplicateException(string message) : base(message)
        {
        }
    }
}
