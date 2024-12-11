using Drawie.Backend.Core.Surfaces.ImageData;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class ChangeProcessingColorSpace_Change : Change
{
    private ColorSpace toColorSpace;
    private ColorSpace original;
    
    [GenerateMakeChangeAction]
    public ChangeProcessingColorSpace_Change(ColorSpace newColorSpace)
    {
        this.toColorSpace = newColorSpace;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        original = target.ProcessingColorSpace;
        return true;  
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        target.ProcessingColorSpace = toColorSpace;

        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ProcessingColorSpace = original;
        return new None();
    }
}
