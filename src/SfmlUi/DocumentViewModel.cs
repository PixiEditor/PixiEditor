using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using SFML.Graphics;

namespace SfmlUi;
internal class DocumentViewModel
{
    public Dictionary<ChunkResolution, BufferBackedTexture> Textures { get; private set; } = new();

    public DocumentChangeTracker Tracker { get; private set; }
    public ActionAccumulator ActionAccumulator { get; private set; }
    public Viewport? Viewport { get; set; }
    public VecI Size { get; }

    public DocumentViewModel(VecI size)
    {
        Textures.Add(ChunkResolution.Full, CreateTexture(size));
        Textures.Add(ChunkResolution.Half, CreateTexture((VecI)(size * ChunkResolution.Half.Multiplier())));
        Textures.Add(ChunkResolution.Quarter, CreateTexture((VecI)(size * ChunkResolution.Quarter.Multiplier())));
        Textures.Add(ChunkResolution.Eighth, CreateTexture((VecI)(size * ChunkResolution.Eighth.Multiplier())));

        Tracker = new DocumentChangeTracker();
        ActionAccumulator = new(this);

        ActionAccumulator.AddFinishedActions(
            new ResizeCanvas_Action(size, PixiEditor.ChangeableDocument.Enums.ResizeAnchor.TopLeft),
            new CreateStructureMember_Action(Tracker.Document.StructureRoot.GuidValue, Guid.NewGuid(), 0, PixiEditor.ChangeableDocument.Enums.StructureMemberType.Layer)
            );
        Size = size;
    }

    public void Draw(VecI pos)
    {
        ActionAccumulator.AddActions(new LineBasedPen_Action(Tracker.Document.StructureRoot.Children[0].GuidValue, Colors.Red, pos, 1, false, false));
    }

    public void StopDrawing()
    {
        ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }

    private BufferBackedTexture CreateTexture(VecI size)
    {
        return new BufferBackedTexture(new(Math.Max(size.X, 1), Math.Max(size.Y, 1)));
    }
}
