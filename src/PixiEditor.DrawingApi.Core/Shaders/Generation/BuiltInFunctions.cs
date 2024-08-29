using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public class BuiltInFunctions
{
    private readonly List<BuiltInFunctionType> usedFunctions = new(Enum.GetValues(typeof(BuiltInFunctionType)).Length);

    public Expression GetRgbToHsv(Expression rgba)
    {
        Require(BuiltInFunctionType.RgbToHsv);

        return new Expression($"{nameof(RgbToHsv)}({rgba.ExpressionValue})");
    }
    
    public Expression GetRgbToHsv(Expression r, Expression g, Expression b, Expression a) =>
        GetRgbToHsv(Half4.Constructor(r, g, b, a));

    public Expression GetHsvToRgb(Expression hsva)
    {
        Require(BuiltInFunctionType.HsvToRgb);

        return new Expression($"{nameof(HsvToRgb)}({hsva.ExpressionValue})");
    }
    
    public Expression GetHsvToRgb(Expression h, Expression s, Expression v, Expression a) =>
        GetHsvToRgb(Half4.Constructor(h, s, v, a));

    public Expression GetHslToRgb(Expression hsla)
    {
        Require(BuiltInFunctionType.HslToRgb);
        
        return new Expression($"{nameof(HslToRgb)}({hsla.ExpressionValue})");
    }

    public Expression GetRgbToHsl(Expression rgba)
    {
        Require(BuiltInFunctionType.RgbToHsl);

        return new Expression($"{nameof(RgbToHsl)}({rgba.ExpressionValue})");
    }

    public Expression GetHslToRgb(Expression h, Expression s, Expression l, Expression a) =>
        GetHslToRgb(Half4.Constructor(h, s, l, a));
    
    public string BuildFunctions()
    {
        var builder = new StringBuilder();

        AppendIf(BuiltInFunctionType.HueToRgb, HueToRgb);
        AppendIf(BuiltInFunctionType.RgbToHcv, RgbToHcv);
        
        AppendIf(BuiltInFunctionType.HsvToRgb, HsvToRgb);
        AppendIf(BuiltInFunctionType.RgbToHsv, RgbToHsv);

        AppendIf(BuiltInFunctionType.HslToRgb, HslToRgb);
        AppendIf(BuiltInFunctionType.RgbToHsl, RgbToHsl);
        
        return builder.ToString();

        void AppendIf(BuiltInFunctionType type, string source)
        {
            if (usedFunctions.Contains(type))
            {
                builder.AppendLine(source);
            }
        }
    }

    private void Require(BuiltInFunctionType type)
    {
        if (usedFunctions.Contains(type))
        {
            return;
        }

        switch (type)
        {
            case BuiltInFunctionType.HsvToRgb:
            case BuiltInFunctionType.HslToRgb:
                Require(BuiltInFunctionType.HueToRgb);
                break;
            case BuiltInFunctionType.RgbToHsv:
            case BuiltInFunctionType.RgbToHsl:
                Require(BuiltInFunctionType.RgbToHcv);
                break;
        }

        usedFunctions.Add(type);
    }

    // Taken from here https://www.shadertoy.com/view/4dKcWK
    private const string HueToRgb =
        $$"""
          half3 {{nameof(HueToRgb)}}(float hue)
          {
              half3 rgb = abs(hue * 6. - half3(3, 2, 4)) * half3(1, -1, -1) + half3(-1, 2, 2);
              return clamp(rgb, 0., 1.);
          }
          """;
    
    private const string RgbToHcv =
        $$"""
          half3 {{nameof(RgbToHcv)}}(half3 rgba)
          {
              half4 p = (rgba.g < rgba.b) ? half4(rgba.bg, -1., 2. / 3.) : half4(rgba.gb, 0., -1. / 3.);
              half4 q = (rgba.r < p.x) ? half4(p.xyw, rgba.r) : half4(rgba.r, p.yzx);
              float c = q.x - min(q.w, q.y);
              float h = abs((q.w - q.y) / (6. * c) + q.z);
              return half3(h, c, q.x);
          }
          """;

    private const string RgbToHsv =
        $$"""
          half4 {{nameof(RgbToHsv)}}(half4 rgba)
          {
              half3 hcv = {{nameof(RgbToHcv)}}(rgba.xyz);
              float s = hcv.y / (hcv.z);
              return half4(hcv.x, s, hcv.z, rgba.w);
          }
          """;

    private const string HsvToRgb =
        $$"""
          half4 {{nameof(HsvToRgb)}}(half4 hsva)
          {
              half3 rgb = {{nameof(HueToRgb)}}(hsva.x);
              return half4(((rgb - 1.) * hsva.y + 1.) * hsva.z, hsva.w);
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

    private const string RgbToHsl =
        $$"""
          half4 {{nameof(RgbToHsl)}}(half4 rgba)
          {
              half3 hcv = {{nameof(RgbToHcv)}}(rgba.xyz);
              half z = hcv.z - hcv.y * 0.5;
              half s = hcv.y / (1. - abs(z * 2. - 1.));
              return half4(hcv.x, s, z, rgba.w);
          }
          """;
    
    enum BuiltInFunctionType
    {
        HueToRgb,
        RgbToHcv,

        RgbToHsv,
        HsvToRgb,
        RgbToHsl,
        HslToRgb
    }
}
