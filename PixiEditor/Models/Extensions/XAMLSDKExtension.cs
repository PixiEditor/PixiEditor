using PixiEditor.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Models.Extensions
{
    public class XAMLSDKExtension : Extension
    {
        public override string Name { get => XAMLName; }

        public string XAMLName { get; set; }

        public override string DisplayName { get => XAMLDisplayName; }

        public string XAMLDisplayName { get; set; }

        public override string Description { get => XAMLDescription; }

        public string XAMLDescription { get; set; }

        public override Version Version => throw new NotImplementedException();

        public override FrameworkElement ExtensionPage { get; }

        public override ImageSource Icon => IconSource;

        public ImageSource IconSource { get; set; }

        public XAMLSDKExtension()
        {
            TextBlock textBlock = new TextBlock
            {
                Text = Description
            };

            ExtensionPage = textBlock;
        }

        public override bool IsVersionSupported(Version pixiEditorVersion)
        {
            throw new NotImplementedException();
        }

        public override void Load(ExtensionLoadingInformation information)
        {
            throw new NotImplementedException();
        }
    }
}
