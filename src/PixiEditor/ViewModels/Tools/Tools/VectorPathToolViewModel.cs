using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;

namespace PixiEditor.ViewModels.Tools.Tools;

internal class VectorPathToolViewModel : ShapeTool, IVectorPathToolHandler
{
    public override string ToolNameLocalizationKey => "PATH_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = [typeof(IVectorLayerHandler)];
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);
    public override LocalizedString Tooltip => new LocalizedString("PATH_TOOL_TOOLTIP", Shortcut);

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorPathTool();
    }
}
