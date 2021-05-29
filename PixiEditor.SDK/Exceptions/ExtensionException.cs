using System;

namespace PixiEditor.SDK
{

    [Serializable]
    public class ExtensionException : Exception
    {
        public string ExtensionPath { get; set; }

        public Extension Extension { get; set; }

        public ExtensionException(string extensionPath) : base()
        {
            ExtensionPath = extensionPath;
        }

        public ExtensionException(string extensionPath, string message) : base(message)
        {
            ExtensionPath = extensionPath;
        }

        public ExtensionException(string extensionPath, string message, Exception inner) : base(message, inner)
        {
            ExtensionPath = extensionPath;
        }

        public ExtensionException(Extension extension)
        {
            Extension = extension;
        }

        public ExtensionException(Extension extension, string message) : base(message)
        {
            Extension = extension;
        }

        public ExtensionException(Extension extension, string message, Exception inner) : base(message, inner) 
        {
            Extension = extension;
        }

        protected ExtensionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
