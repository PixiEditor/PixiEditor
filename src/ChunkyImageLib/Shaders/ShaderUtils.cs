using ComputeSharp;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using SkiaSharp;

namespace ChunkyImageLib.Shaders;

public static class ShaderUtils
{
    public static float4 UnpackPixel(uint2 packedPixel)
    {
        return new float4(
            Hlsl.Float16ToFloat32(packedPixel.X),
            Hlsl.Float16ToFloat32(packedPixel.X >> 16),
            Hlsl.Float16ToFloat32(packedPixel.Y),
            Hlsl.Float16ToFloat32(packedPixel.Y >> 16)
        );
    }

    public static uint2 PackPixel(Color color)
    {
        uint convR = (BitConverter.HalfToUInt16Bits((Half)(color.R / 255f)));
        uint convG = (BitConverter.HalfToUInt16Bits((Half)(color.G / 255f)));
        uint convB = (BitConverter.HalfToUInt16Bits((Half)(color.B / 255f)));
        uint convA = (BitConverter.HalfToUInt16Bits((Half)(color.A / 255f)));

        return new UInt2(convG << 16 | convR, convB | convA << 16);
    }
}
