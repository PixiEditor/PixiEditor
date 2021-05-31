using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PixiEditor.SDK
{
    [DebuggerDisplay("{DisplayName,nq} ({Name,nq})")]
    public abstract class Extension
    {
        public abstract string Name { get; }

        public abstract string DisplayName { get; }

        public abstract string Description { get; }

        public string ExtensionPath { get; internal set; }

        public abstract Version Version { get; }

        public Preferences Preferences { get; internal set; }

        internal List<string> SupportedDocumentFileExtensions { get; set; } = new List<string>();

        internal List<string> SupportedImageFileExtensions { get; set; } = new List<string>();

        public abstract bool IsVersionSupported(Version pixiEditorVersion);

        public abstract void Load(ExtensionLoadingInformation information);
    }
}
