using System.Collections;
using System.Collections.Specialized;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("COLOR_RAMP_NODE", "IMAGE", PixiPerfectIcons.Gradient)]
internal class ColorRampNodeViewModel : NodeViewModel<ColorRampNode>;
