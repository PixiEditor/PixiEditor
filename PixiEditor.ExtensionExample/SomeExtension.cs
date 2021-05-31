using PixiEditor.ExtensionExample;
using PixiEditor.SDK;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[assembly: PixiEditorExtension(typeof(SomeExtension))]

namespace PixiEditor.ExtensionExample
{
    public class SomeExtension : Extension
    {
        public override string Name { get; } = "PixiEditor.ExampleExtension";

        public override string DisplayName { get; } = "Example Extension";

        public override string Description { get; } = "A exmaple extension showing how extensions work";

        public override FrameworkElement ExtensionPage { get; } = new ExtensionPage();

        public override Version Version { get; } = new Version(4, 2, 0, 69);

        public override ImageSource Icon { get; }

        public override bool IsVersionSupported(Version pixiEditorVersion) => true;

        public SomeExtension()
        {
            Icon = LoadImageFromResource("./PixiExampleLogo.png");
        }

        public override void Load(ExtensionLoadingInformation information)
        {
            if (Preferences.GetLocalPreference("Test", true))
            {
                Preferences.UpdateLocalPreference("Test", false);
            }

            information
                .AddDocumentParser<ExampleDocumentParser>();
        }
    }
}
