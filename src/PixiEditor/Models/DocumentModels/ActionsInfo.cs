using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.DocumentPassthroughActions;

namespace PixiEditor.Models.DocumentModels;

public struct ActionsInfo
{
    public bool UndoBoundaryPassed { get; set; }
    public bool ViewportRefreshRequested { get; set; }
    public bool PreviewsRefreshRequested { get; set; }
    public bool PreviewRefreshRequested { get; set; }
    public bool ChangeFrameRequested { get; set; }
    public bool DebugRenderRequested { get; set; }
    public bool GenerateBrushPreviewRequested { get; set; }
    public List<GenerateBrushPreview_PassthroughAction> BrushPreviewActions { get; }

    public ActionsInfo(List<(ActionSource source, IAction action)> executed)
    {
        foreach (var act in executed)
        {
            switch (act.action)
            {
                case ChangeBoundary_Action or Redo_Action or Undo_Action:
                    UndoBoundaryPassed = true;
                    continue;
                case RefreshViewport_PassthroughAction:
                    PreviewsRefreshRequested = true;
                    continue;
                case RefreshPreviews_PassthroughAction:
                    PreviewsRefreshRequested = true;
                    continue;
                case RefreshPreview_PassthroughAction:
                    PreviewRefreshRequested = true;
                    continue;
                case SetActiveFrame_PassthroughAction:
                    ChangeFrameRequested = true;
                    continue;
                case DebugRecordFrame_PassthroughAction:
                    DebugRenderRequested = true;
                    continue;
                case GenerateBrushPreview_PassthroughAction brushAct:
                    GenerateBrushPreviewRequested = true;
                    BrushPreviewActions ??= new List<GenerateBrushPreview_PassthroughAction>();
                    BrushPreviewActions.Add(brushAct);
                    continue;
            }
        }
    }
}
