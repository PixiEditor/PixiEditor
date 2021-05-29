using System;

namespace PixiEditor.SDK
{
    public class ExtensionLoadingException : ExtensionException
    {
        public ExtensionLoadingException(string extensionPath) : base(extensionPath, "Error while trying to load extension") { }

        public ExtensionLoadingException(string extensionPath, string message) : base(extensionPath, message) { }

        public ExtensionLoadingException(string extensionPath, string message, Exception inner) : base(extensionPath, message, inner) { }

        public ExtensionLoadingException(Extension extension) : base(extension, "Error while trying to load extension") { }

        public ExtensionLoadingException(Extension extension, string message) : base(extension, message) { }

        public ExtensionLoadingException(Extension extension, string message, Exception inner) : base(extension, message, inner) { }

        protected ExtensionLoadingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
