using ChangeableDocument.Actions;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace PixiEditorPrototype.Models
{
    internal record class MoveViewport_PassthroughAction : IAction, IChangeInfo
    {
        public MoveViewport_PassthroughAction(SKRect viewport, ChunkResolution resolution)
        {
            Viewport = viewport;
            Resolution = resolution;
        }

        public SKRect Viewport { get; }
        public ChunkResolution Resolution { get; }
    }
}
