using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models.Rendering.RenderInfos;

public record class DirtyRect_RenderInfo(VecI Pos, VecI Size, ChunkResolution Resolution) : IRenderInfo;
