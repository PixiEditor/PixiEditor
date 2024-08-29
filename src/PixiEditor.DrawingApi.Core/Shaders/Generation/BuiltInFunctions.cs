using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public class BuiltInFunctions
{
    private readonly List<BuiltInFunctionType> usedFunctions = new(Enum.GetValues(typeof(BuiltInFunctionType)).Length);

    public Expression GetHslToRgb(Expression hsla)
    {
        Require(BuiltInFunctionType.HslToRgb);
        
        return new Expression($"{nameof(HslToRgb)}({hsla.ExpressionValue})");
    }

    public Expression GetHslToRgb(Expression h, Expression s, Expression l, Expression a) =>
        GetHslToRgb(Half4.Constructor(h, s, l, a));
    
    private void Require(BuiltInFunctionType type)
    {
        if (usedFunctions.Contains(type))
        {
            return;
        }

        if (type is BuiltInFunctionType.HslToRgb)
        {
            Require(BuiltInFunctionType.HueToRgb);
        }
            
        usedFunctions.Add(type);
    }

    public string BuildFunctions()
    {
        var builder = new StringBuilder();

        AppendIf(BuiltInFunctionType.HueToRgb, HueToRgb);
        AppendIf(BuiltInFunctionType.HslToRgb, HslToRgb);
        
        return builder.ToString();

        void AppendIf(BuiltInFunctionType type, string source)
        {
            if (usedFunctions.Contains(type))
            {
                builder.Append(source);
            }
        }
    }
    
    // Taken from here https://www.shadertoy.com/view/4dKcWK
    private const string HueToRgb =
        $$"""
        half3 {{nameof(HueToRgb)}}(float hue)
        {
            vec3 rgb = abs(hue * 6. - vec3(3, 2, 4)) * vec3(1, -1, -1) + vec3(-1, 2, 2);
            return clamp(rgb, 0., 1.);
        }
        """;
    
    private const string HslToRgb = 
        $$"""
        half4 {{nameof(HslToRgb)}}(half4 hsla)
        {
            half3 rgb = {{nameof(HueToRgb)}}(hsla.x);
            float c = (1. - abs(2. * hsla.z - 1.)) * hsla.y;
            return half4((rgb - 0.5) * c + hsla.z, hsla.w);
        }
        """;
    
    // private const string RgbToHcv = 
    //     """
    //     half4 RGBtoHSL(half4 rgba)
    //     {
    //         half4 p = (rgba.g < rgba.b) ? half4(rgba.bg, -1., 2. / 3.) : half4(rgba.gb, 0., -1. / 3.);
    //         half4 q = (rgba.r < p.x) ? half4(p.xyw, rgba.r) : half4(rgba.r, p.yzx);
    //         float c = q.x - min(q.w, q.y);
    //         float h = abs((q.w - q.y) / (6. * c + EPSILON) + q.z);
    //         return half4(h, c, q.x, rgba.w);
    //     }
    //     """;
    //
    // private const string RgbToHsl = 
    //     """
    //     half4 RGBtoHSL(half4 rgba)
    //     {
    //         // RGB [0..1] to Hue-Saturation-Lightness [0..1]
    //         half3 hcv = RGBtoHCV(rgba.xyz);
    //         half z = hcv.z - hcv.y * 0.5;
    //         half s = hcv.y / (1. - abs(z * 2. - 1.) + EPSILON);
    //         return half4(hcv.x, s, z, rgba.w);
    //     }
    //     """;
}
