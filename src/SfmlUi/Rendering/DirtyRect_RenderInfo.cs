using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace SfmlUi.Rendering;
#nullable enable
public record class DirtyRect_RenderInfo(VecI Pos, VecI Size, ChunkResolution Resolution);
