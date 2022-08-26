using ComputeSharp;
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

    public static uint2 PackPixel(SKColor color)
    {
        uint convR = (BitConverter.HalfToUInt16Bits((Half)(color.Red / 255f)));
        uint convG = (BitConverter.HalfToUInt16Bits((Half)(color.Green / 255f)));
        uint convB = (BitConverter.HalfToUInt16Bits((Half)(color.Blue / 255f)));
        uint convA = (BitConverter.HalfToUInt16Bits((Half)(color.Alpha / 255f)));

        return new UInt2(convG << 16 | convR, convB | convA << 16);
    }
}
