using System.Collections.Generic;
using System.Text;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public class BuiltInFunctions
{
    private readonly List<IBuiltInFunction> usedFunctions = new(6);

    public Expression GetRgbToHsv(Expression rgba) => Call(RgbToHsv, rgba);

    public Expression GetRgbToHsl(Expression rgba) => Call(RgbToHsl, rgba);

    public Expression GetHsvToRgb(Expression hsva) => Call(HsvToRgb, hsva);
    
    public Expression GetHsvToRgb(Expression h, Expression s, Expression v, Expression a) =>
        GetHsvToRgb(Half4.Constructor(h, s, v, a));

    public Expression GetHslToRgb(Expression hsla) => Call(HslToRgb, hsla);

    public Expression GetHslToRgb(Expression h, Expression s, Expression l, Expression a) =>
        GetHslToRgb(Half4.Constructor(h, s, l, a));

    public string BuildFunctions()
    {
        var builder = new StringBuilder();

        AppendIf(HueToRgb);
        AppendIf(RgbToHcv);
        
        AppendIf(HsvToRgb);
        AppendIf(RgbToHsv);

        AppendIf(HslToRgb);
        AppendIf(RgbToHsl);
        
        return builder.ToString();

        void AppendIf(IBuiltInFunction function)
        {
            if (usedFunctions.Contains(function))
            {
                builder.AppendLine(function.FullSource);
            }
        }
    }

    private Expression Call(IBuiltInFunction function, Expression expression)
    {
        Require(function);

        return new Expression(function.Call(expression.ExpressionValue));
    }

    private void Require(IBuiltInFunction function)
    {
        if (usedFunctions.Contains(function))
        {
            return;
        }

        foreach (var dependency in function.Dependencies)
        {
            Require(dependency);
        }

        usedFunctions.Add(function);
    }

    // Taken from here https://www.shadertoy.com/view/4dKcWK
    private static readonly BuiltInFunction<Half3> HueToRgb = new(
        "float hue",
        nameof(HueToRgb),
        """
        half3 rgb = abs(hue * 6. - half3(3, 2, 4)) * half3(1, -1, -1) + half3(-1, 2, 2);
        return clamp(rgb, 0., 1.);
        """);

    private static readonly BuiltInFunction<Half3> RgbToHcv = new(
        "half3 rgba",
        nameof(RgbToHcv),
        """
        half4 p = (rgba.g < rgba.b) ? half4(rgba.bg, -1., 2. / 3.) : half4(rgba.gb, 0., -1. / 3.);
        half4 q = (rgba.r < p.x) ? half4(p.xyw, rgba.r) : half4(rgba.r, p.yzx);
        float c = q.x - min(q.w, q.y);
        float h = abs((q.w - q.y) / (6. * c) + q.z);
        return half3(h, c, q.x);
        """);

    private static readonly BuiltInFunction<Half4> RgbToHsv = new(
        "half4 rgba",
        nameof(RgbToHsv),
        $"""
         half3 hcv = {RgbToHcv.Call("rgba.rgb")};
         float s = hcv.y / (hcv.z);
         return half4(hcv.x, s, hcv.z, rgba.w);
         """,
        RgbToHcv);

    private static readonly BuiltInFunction<Half4> HsvToRgb = new(
        "half4 hsva",
        nameof(HsvToRgb),
        $"""
         half3 rgb = {HueToRgb.Call("hsva.r")};
         return half4(((rgb - 1.) * hsva.y + 1.) * hsva.z, hsva.w);
         """,
        HueToRgb);

    private static readonly BuiltInFunction<Half4> RgbToHsl = new(
        "half4 rgba", 
        nameof(RgbToHsl), 
        $"""
         half3 hcv = {RgbToHcv.Call("rgba.rgb")};
         half z = hcv.z - hcv.y * 0.5;
         half s = hcv.y / (1. - abs(z * 2. - 1.));
         return half4(hcv.x, s, z, rgba.w);
         """,
        RgbToHcv);

    private static readonly BuiltInFunction<Half4> HslToRgb = new(
        "half4 hsla",
        nameof(HslToRgb),
        $"""
         half3 rgb = {HueToRgb.Call("hsla.r")};
         float c = (1. - abs(2. * hsla.z - 1.)) * hsla.y;
         return half4((rgb - 0.5) * c + hsla.z, hsla.w);
         """,
        HueToRgb);

    private class BuiltInFunction<TReturn>(string argumentList, string name, string body, params IBuiltInFunction[] dependencies) : IBuiltInFunction where TReturn : ShaderExpressionVariable
    {
        public string ArgumentList { get; } = argumentList;

        public string Name { get; } = name;

        public string Body { get; } = body;

        public IBuiltInFunction[] Dependencies { get; } = dependencies;

        public string FullSource =>
         $$"""
          {{typeof(TReturn).Name.ToLower()}} {{Name}}({{ArgumentList}}) {
          {{Body}}
          }
          """;

        public string Call(string arguments) => $"{Name}({arguments})";
    }
    
    private interface IBuiltInFunction
    {
        IBuiltInFunction[] Dependencies { get; }
        
        string Name { get; }
        
        string FullSource { get; }

        string Call(string arguments);
    }
}
