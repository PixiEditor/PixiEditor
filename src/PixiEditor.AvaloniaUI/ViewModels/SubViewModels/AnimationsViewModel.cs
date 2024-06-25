using ChunkyImageLib;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Animations", "ANIMATIONS")]
internal class AnimationsViewModel : SubViewModel<ViewModelMain>
{
    public AnimationsViewModel(ViewModelMain owner) : base(owner)
    {
    }
    
    [Command.Basic("PixiEditor.Animation.CreateRasterKeyFrame", "Create Raster Key Frame", "Create a raster key frame", Parameter = false)]
    [Command.Basic("PixiEditor.Animation.DuplicateRasterKeyFrame", "Duplicate Raster Key Frame", "Duplicate a raster key frame", Parameter = true)]
    public void CreateRasterKeyFrame(bool duplicate)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument?.SelectedStructureMember is null)
        {
            return;
        }

        int newFrame = GetActiveFrame(activeDocument, activeDocument.SelectedStructureMember.GuidValue);
        
        activeDocument.AnimationDataViewModel.CreateRasterKeyFrame(
            activeDocument.SelectedStructureMember.GuidValue, 
            newFrame,
            duplicate);
        
        activeDocument.Operations.SetActiveFrame(newFrame);
    }
    
    [Command.Basic("PixiEditor.Animation.DeleteKeyFrames", "Delete key frames", "Delete key frames")]
    public void DeleteKeyFrames(IList<KeyFrameViewModel> keyFrames)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (activeDocument is null)
            return;
        
        List<Guid> keyFrameIds = keyFrames.Select(x => x.Id).ToList();
        
        for(int i = 0; i < keyFrameIds.Count; i++)
        {
            if(!activeDocument.AnimationDataViewModel.TryFindKeyFrame<KeyFrameViewModel>(keyFrameIds[i], out _))
            {
                keyFrameIds.RemoveAt(i);
                i--;   
            }
        }
        
        activeDocument.AnimationDataViewModel.DeleteKeyFrames(keyFrameIds);
    }

    [Command.Internal("PixiEditor.Animation.ChangeKeyFramesStartPos")]
    public void ChangeKeyFramesStartPos((Guid[] ids, int delta, bool end) info)
    {
        var activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (activeDocument is null)
            return;

        if (!info.end)
        {
            activeDocument.AnimationDataViewModel.ChangeKeyFramesStartPos(info.ids, info.delta);
        }
        else
        {
            activeDocument.AnimationDataViewModel.EndKeyFramesStartPos();
        }
    }
    
    [Command.Basic("PixiEditor.Animation.ExportSpriteSheet", "Export Sprite Sheet", "Export the sprite sheet")]
    public async Task ExportSpriteSheet()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        
        if (document is null)
            return;
        
        Image[] images = document.RenderFrames();
        // calculate rows and columns so as little empty space is left
        // For example 3 frames should be in 3x1 grid because 2x2 would leave 1 empty space, but 4 frames should be in 2x2 grid
        (int rows, int columns) grid = CalculateGridDimensions(images.Length);
        
        using Surface surface = new Surface(new VecI(document.Width * grid.columns, document.Height * grid.rows));
        for (int i = 0; i < images.Length; i++)
        {
            int x = i % grid.columns;
            int y = i / grid.columns;
            surface.DrawingSurface.Canvas.DrawImage(images[i], x * document.Width, y * document.Height);
        }
        surface.SaveToDesktop();
    }

    private (int rows, int columns) CalculateGridDimensions(int imagesLength)
    {
        int optimalRows = 1;
        int optimalColumns = imagesLength;
        int minDifference = Math.Abs(optimalRows - optimalColumns);

        for (int rows = 1; rows <= Math.Sqrt(imagesLength); rows++)
        {
            int columns = (int)Math.Ceiling((double)imagesLength / rows);

            if (rows * columns >= imagesLength)
            {
                int difference = Math.Abs(rows - columns);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    optimalRows = rows;
                    optimalColumns = columns;
                }
            }
        }

        return (optimalRows, optimalColumns);
    }

    private static int GetActiveFrame(DocumentViewModel activeDocument, Guid targetLayer)
    {
        int active = activeDocument.AnimationDataViewModel.ActiveFrameBindable;
        if (activeDocument.AnimationDataViewModel.TryFindKeyFrame<KeyFrameGroupViewModel>(targetLayer, out KeyFrameGroupViewModel groupViewModel))
        {
            if(active == groupViewModel.StartFrameBindable + groupViewModel.DurationBindable - 1)
            {
                return groupViewModel.StartFrameBindable + groupViewModel.DurationBindable;
            }
        }
        
        return active;
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
