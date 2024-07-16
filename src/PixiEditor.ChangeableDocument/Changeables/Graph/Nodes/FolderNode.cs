using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
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

    public override bool Validate()
    {
        return true;
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };

    protected override Surface? OnExecute(RenderingContext context)
    {
        if (!IsVisible.Value || Content.Value == null)
        {
            Output.Value = Background.Value;
            return Output.Value;
        }

        VecI size = Content.Value?.Size ?? Background.Value.Size;
        
        Surface workingSurface = new Surface(size);

        if (Background.Value != null)
        {
            workingSurface.DrawingSurface.Canvas.DrawSurface(Background.Value.DrawingSurface, 0, 0);
        }

        if (Content.Value != null)
        {
            workingSurface.DrawingSurface.Canvas.DrawSurface(Content.Value.DrawingSurface, 0, 0);
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
