using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering.RenderInfos;
#nullable enable
public record class DirtyRect_RenderInfo(VecI Pos, VecI Size, ChunkResolution Resolution) : IRenderInfo;
