using Avalonia.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.T)]
internal class TextToolViewModel : ToolViewModel, ITextToolHandler
{
    public override string ToolNameLocalizationKey => "TEXT_TOOL";
    public override Type[]? SupportedLayerTypes => [];
    public override Type LayerTypeToCreateOnEmptyUse => typeof(VectorLayerNode);
    public override LocalizedString Tooltip => new LocalizedString("TEXT_TOOL_TOOLTIP");

    public override bool IsErasable => false;
    public override bool StopsLinkedToolOnUse => false;

    [Settings.Inherited]
    public double FontSize
    {
        get => GetValue<double>();
    }
    
    public TextToolViewModel()
    {
        Toolbar = ToolbarFactory.Create<TextToolViewModel, TextToolbar>(this);
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorTextTool();
    }

    protected override void OnSelected(bool restoring)
    {
        if (!restoring)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorTextTool();
        }
    }

    protected override void OnDeselecting(bool transient)
    {
        if (!transient)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnSelected(false);
    }
}
