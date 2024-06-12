using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Animations", "ANIMATIONS")]
internal class AnimationsViewModel : SubViewModel<ViewModelMain>
{
    public AnimationsViewModel(ViewModelMain owner) : base(owner)
    {
        
    }
    
    [Command.Basic("PixiEditor.Animation.CreateRasterKeyFrame", "Create Raster Key Frame", "Create a raster key frame")]
    public void CreateRasterClip()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument == null)
        {
            return;
        }

        int newFrame = 0;

        if (activeDocument.AnimationDataViewModel.TryFindKeyFrame(activeDocument.SelectedStructureMember.GuidValue, out KeyFrameGroupViewModel group))
        {
            newFrame = group.StartFrame + group.Duration;
        }
        
        activeDocument.AnimationDataViewModel.CreateRasterKeyFrame(
            activeDocument.SelectedStructureMember.GuidValue, 
            newFrame,
            false);
    }
    
    [Command.Internal("PixiEditor.Document.StartChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void StartChangeActiveFrame(int newActiveFrame)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        
        Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.StartChangeActiveFrame();
        Owner.DocumentManagerSubViewModel.ActiveDocument.Tools.UseActiveFrame(newActiveFrame);
    }
    
    [Command.Internal("PixiEditor.Document.ChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void ChangeActiveFrame(double newActiveFrame)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        
        int intNewActiveFrame = (int)newActiveFrame;
        
        Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.ChangeActiveFrame(intNewActiveFrame);
    }

    [Command.Internal("PixiEditor.Document.EndChangeActiveFrame", CanExecute = "PixiEditor.HasDocument")]
    public void EndChangeActiveFrame()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        
        Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.EndChangeActiveFrame();
    }
    
    [Command.Internal("PixiEditor.Animation.ActiveFrameSet")]
    public void ActiveFrameSet(double value)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        
        if (document is null)
            return;

        document.Operations.SetActiveFrame((int)value);
    }
}
