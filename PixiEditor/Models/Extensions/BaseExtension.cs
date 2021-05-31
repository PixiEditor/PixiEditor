using PixiEditor.Models.IO.Parsers;
using PixiEditor.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models
{
    class BaseExtension : Extension
    {
        public override string Name => "PixiEditor.PixiEditor";

        public override string DisplayName => "PixiEditor";

        public override string Description => "Extension containing parser's for .pixi, .png and .jpg files";

        public override Version Version => new Version(1, 0, 0, 0);

        public override FrameworkElement ExtensionPage { get; }

        public override ImageSource Icon { get; }

        public override bool IsVersionSupported(Version pixiEditorVersion) => true;

        public BaseExtension()
        {
            ExtensionPage = CreatePage();
            Icon = LoadImageFromResource("Images/PixiEditorLogo.png");
        }

        public override void Load(ExtensionLoadingInformation information)
        {
            information
                .AddDocumentParser<PixiParser>()
                .AddImageParser<PngParser>()
                .AddImageParser<JpegParser>();
        }

        private FrameworkElement CreatePage()
        {
            TextBlock block = new TextBlock();
            block.Text = Description;
            return block;
        }
    }
}
