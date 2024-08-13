using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder", "FOLDER_NODE")]
public class FolderNode : StructureNode, IReadOnlyFolderNode
{
    public InputProperty<Texture?> Content { get; }

    public FolderNode()
    {
        Content = CreateInput<Texture?>("Content", "CONTENT", null);
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };


    protected override Texture? OnExecute(RenderingContext context)
    {
        if(Background.Value == null && Content.Value == null)
        {
            Output.Value = null;
            return null;
        }
        
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return Output.Value;
        }
        
        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

        VecI size = Content.Value?.Size ?? Background.Value?.Size ?? VecI.Zero;
        
        var outputWorkingSurface = TryInitWorkingSurface(size, context, 0);
        var filterlessWorkingSurface = TryInitWorkingSurface(size, context, 1);
        
        if (Background.Value != null)
        {
            DrawBackground(filterlessWorkingSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
        }

        if (Content.Value != null)
        {
            blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255)); 
            DrawSurface(filterlessWorkingSurface, Content.Value, context, null);
        }

        FilterlessOutput.Value = filterlessWorkingSurface;

        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                blendPaint.Color = new Color(255, 255, 255, 255);
                blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;
                DrawBackground(outputWorkingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }
            
            if (Content.Value != null)
            {
                blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255)); 
                DrawSurface(outputWorkingSurface, Content.Value, context, Filters.Value);
            }
            
            Output.Value = outputWorkingSurface;
            return Output.Value;
        }
        
        if (Content.Value != null)
        {
            DrawSurface(outputWorkingSurface, Content.Value, context, Filters.Value);
            
            ApplyMaskIfPresent(outputWorkingSurface, context);
        }
        
        if (Background.Value != null)
        {
            Texture tempSurface = RequestTexture(2, outputWorkingSurface.Size);
            DrawBackground(tempSurface, context);
            
            ApplyRasterClip(outputWorkingSurface, tempSurface);
            
            blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface;
            return tempSurface;
        }

        Output.Value = outputWorkingSurface;
        return Output.Value;
    }

    public override RectI? GetTightBounds(KeyFrameTime frameTime)
    {
        // TODO: Implement GetTightBounds
        return RectI.Create(0, 0, Content.Value?.Size.X ?? 0, Content.Value?.Size.Y ?? 0);
        /*if (Children.Count == 0)
      {
          return null;
      }

      var bounds = Children[0].GetTightBounds(frame);
      for (var i = 1; i < Children.Count; i++)
      {
          var childBounds = Children[i].GetTightBounds(frame);
          if (childBounds == null)
          {
              continue;
          }

          if (bounds == null)
          {
              bounds = childBounds;
          }
          else
          {
              bounds = bounds.Value.Union(childBounds.Value);
          }
      }

      return bounds;*/
    }

    /// <summary>
    /// Creates a clone of the folder, its mask and all of its children
    /// </summary>
    /*internal override Folder Clone()
    {
        var builder = ImmutableList<StructureMember>.Empty.ToBuilder();
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            builder.Add(child.Clone());
        }

        return new Folder
        {
            GuidValue = GuidValue,
            IsVisible = IsVisible,
            Name = Name,
            Opacity = Opacity,
            Children = builder.ToImmutable(),
            Mask = Mask?.CloneFromCommitted(),
            BlendMode = BlendMode,
            ClipToMemberBelow = ClipToMemberBelow,
            MaskIsVisible = MaskIsVisible
        };
    }*/
}
