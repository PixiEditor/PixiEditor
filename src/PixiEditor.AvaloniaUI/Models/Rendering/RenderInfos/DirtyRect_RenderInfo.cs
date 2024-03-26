using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Rendering.RenderInfos;
#nullable enable
public record class DirtyRect_RenderInfo(VecI Pos, VecI Size, ChunkResolution Resolution) : IRenderInfo;
