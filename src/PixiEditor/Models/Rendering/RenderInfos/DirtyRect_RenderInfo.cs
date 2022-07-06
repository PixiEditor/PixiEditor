using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.Rendering.RenderInfos;
#nullable enable
public record class DirtyRect_RenderInfo(VecI Pos, VecI Size, ChunkResolution Resolution) : IRenderInfo;
