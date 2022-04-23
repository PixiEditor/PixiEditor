using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models.Rendering.RenderInfos
{
    public record class DirtyRect_RenderInfo(Vector2i Pos, Vector2i Size, ChunkResolution Resolution) : IRenderInfo;
}
