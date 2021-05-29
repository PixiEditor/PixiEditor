using PixiEditor.ExtensionExample;
using PixiEditor.SDK;
using System;

[assembly: PixiEditorExtension(typeof(SomeExtension))]

namespace PixiEditor.ExtensionExample
{
    public class SomeExtension : Extension
    {
        public override string Name { get; } = "PixiEditor.ExampleExtension";

        public override string DisplayName { get; } = "Example extension";

        public override string Description { get; } = "A exmaple extension showing how extensions work";

        public override Version Version { get; } = new Version(1, 0, 0, 0);

        public override bool IsVersionSupported(Version pixiEditorVersion) => true;

        public override void Load(ExtensionLoadingInformation information)
        {
            information
                .AddDocumentParser<ExampleDocumentParser>();
        }
    }
}
