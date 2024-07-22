using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class FolderNode : StructureNode, IReadOnlyFolderNode
{
    public InputProperty<Surface?> Content { get; }

    public FolderNode()
    {
        Content = CreateInput<Surface?>("Content", "CONTENT", null);
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };

    protected override string NodeUniqueName => "Folder";

    protected override Surface? OnExecute(RenderingContext context)
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
        blendPaint.BlendMode = DrawingApi.Core.Surface.BlendMode.Src;

        VecI size = Content.Value?.Size ?? Background.Value?.Size ?? VecI.Zero;
        
        var workingSurface = TryInitWorkingSurface(size, context);

        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                DrawBackground(workingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }
            
            if (Content.Value != null)
            {
                blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255)); 
                DrawSurface(workingSurface, Content.Value, context);
            }
            
            Output.Value = workingSurface;
            return Output.Value;
        }
        
        if (Content.Value != null)
        {
            DrawSurface(workingSurface, Content.Value, context);
            
            ApplyMaskIfPresent(workingSurface, context);
            ApplyRasterClip(workingSurface, context);
        }
        
        if (Background.Value != null)
        {
            Surface tempSurface = new Surface(workingSurface.Size);
            DrawBackground(tempSurface, context);
            
            blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(workingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface;
            return tempSurface;
        }

        Output.Value = workingSurface;
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
