using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Animations", "ANIMATIONS")]
internal class AnimationsViewModel : SubViewModel<ViewModelMain>
{
    public AnimationsViewModel(ViewModelMain owner) : base(owner)
    {
        
    }
    
    [Command.Debug("PixiEditor.Animations.CreateRasterClip", "Create Raster Clip", "Create a raster clip")]
    public void CreateRasterClip()
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument == null)
        {
            return;
        }
        
        
        activeDocument.AnimationDataViewModel.AddRasterClip(
            activeDocument.SelectedStructureMember.GuidValue, 
            activeDocument.AnimationDataViewModel.Clips.Count,
            false);
    }
}
