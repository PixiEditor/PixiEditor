using PixiEditor.Models.IO.Parsers;
using PixiEditor.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models
{
    class BaseExtension : Extension
    {
        public override string Name => "PixiEditor.PixiEditorBase";

        public override string DisplayName => "PixiEditor";

        public override string Description => "Extension containing parser's for .pixi, .png and .jpg files";

        public override Version Version => new Version(1, 0, 0, 0);

        public override bool IsVersionSupported(Version pixiEditorVersion) => true;

        public override void Load(ExtensionLoadingInformation information)
        {
            information
                .AddDocumentParser<PixiParser>()
                .AddImageParser<PngParser>()
                .AddImageParser<JpegParser>();
        }
    }
}
