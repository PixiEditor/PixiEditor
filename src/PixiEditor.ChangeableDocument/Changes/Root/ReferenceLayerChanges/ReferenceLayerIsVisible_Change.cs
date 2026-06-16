using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Root.ReferenceLayerChanges;
internal class ReferenceLayerIsVisible_Change : Change
{
    private readonly bool isVisible;
    private bool oldIsVisible;

    [GenerateMakeChangeAction]
    public ReferenceLayerIsVisible_Change(bool isVisible)
    {
        this.isVisible = isVisible;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.ReferenceLayer is null)
            return false;
        oldIsVisible = target.ReferenceLayer.IsVisible;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (oldIsVisible == isVisible)
        {
            ignoreInUndo = true;
            return new None();
        }
        ignoreInUndo = false;
        target.ReferenceLayer!.IsVisible = isVisible;
        return new ReferenceLayerIsVisible_ChangeInfo(isVisible);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ReferenceLayer!.IsVisible = oldIsVisible;
        return new ReferenceLayerIsVisible_ChangeInfo(oldIsVisible);
    }
}
