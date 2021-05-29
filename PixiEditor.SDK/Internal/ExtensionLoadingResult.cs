namespace PixiEditor.SDK
{
    internal struct ExtensionLoadingResult
    {
        public ExtensionLoadingException[] LoadingExceptions { get; }

        public ExtensionLoadingResult(ExtensionLoadingException[] exceptions)
        {
            LoadingExceptions = exceptions;
        }
    }
}
