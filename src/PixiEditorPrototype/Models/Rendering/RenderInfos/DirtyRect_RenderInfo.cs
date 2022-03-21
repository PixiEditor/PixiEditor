using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models.Rendering.RenderInfos
{
    public record struct DirtyRect_RenderInfo : IRenderInfo
    {
        public DirtyRect_RenderInfo(Vector2i pos, Vector2i size)
        {
            Pos = pos;
            Size = size;
        }

        public Vector2i Pos { get; }
        public Vector2i Size { get; }
    }
}
