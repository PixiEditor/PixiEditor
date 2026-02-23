using System.Numerics;
using System.Reflection;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public static class ConversionTable
{
    private static readonly Dictionary<Type, List<(Type, ITypeConverter)>> _conversionTable =
        new()
        {
            {
                typeof(double), [
                    (typeof(int), new TypeConverter<double, int>(DoubleToInt)),
                    (typeof(float), new TypeConverter<double, float>(d => (float)d)),
                    (typeof(VecD), new TypeConverter<double, VecD>(DoubleToVecD)),
                    (typeof(VecI), new TypeConverter<double, VecI>(DoubleToVecI)),
                    (typeof(Vec3D), new TypeConverter<double, Vec3D>(d => new Vec3D(d, d, d))),
                ]
            },
            {
                typeof(float), [
                    (typeof(double), new TypeConverter<float, double>(f => (double)f)),
                    (typeof(int), new TypeConverter<float, int>(f => (int)f)),
                    (typeof(VecD), new TypeConverter<float, VecD>(f => new VecD(f, f))),
                    (typeof(VecI), new TypeConverter<float, VecI>(f => new VecI((int)f, (int)f))),
                    (typeof(Vec3D), new TypeConverter<float, Vec3D>(f => new Vec3D(f, f, f))),
                ]
            },
            {
                typeof(int), [
                    (typeof(double), new TypeConverter<int, double>(ConvertIntToDouble)),
                    (typeof(float), new TypeConverter<int, float>(i => (float)i)),
                    (typeof(VecI), new TypeConverter<int, VecI>(IntToVecI)),
                    (typeof(VecD), new TypeConverter<int, VecD>(IntToVecD)),
                    (typeof(Vec3D), new TypeConverter<int, Vec3D>(i => new Vec3D(i, i, i))),
                ]
            },
            {
                typeof(VecD), [
                    (typeof(double), new TypeConverter<VecD, double>(VecDToDouble)),
                    (typeof(int), new TypeConverter<VecD, int>(VecDToInt)),
                    (typeof(float), new TypeConverter<VecD, float>(v => (float)v.X)),
                    (typeof(VecI), new TypeConverter<VecD, VecI>(VecDToVecI)),
                    (typeof(Vec3D), new TypeConverter<VecD, Vec3D>(v => new Vec3D(v.X, v.Y, v.Y)))
                ]
            },
            {
                typeof(VecI), [
                    (typeof(double), new TypeConverter<VecI, double>(VecIToDouble)),
                    (typeof(int), new TypeConverter<VecI, int>(VecIToInt)),
                    (typeof(float), new TypeConverter<VecI, float>(v => v.X)),
                    (typeof(VecD), new TypeConverter<VecI, VecD>(VecIToVecD)),
                    (typeof(Vec3D), new TypeConverter<VecI, Vec3D>(v => new Vec3D(v.X, v.Y, v.Y)))
                ]
            },
            {
                typeof(Color), [
                    (typeof(Paintable), new TypeConverter<Color, Paintable>(c => new ColorPaintable(c))),
                    (typeof(VecD), new TypeConverter<Color, VecD>(c => new VecD(c.R, c.G))),
                    (typeof(VecI), new TypeConverter<Color, VecI>(c => new VecI(c.R, c.G))),
                    (typeof(Vec3D), new TypeConverter<Color, Vec3D>(c => new Vec3D(c.R, c.G, c.B))),
                    (typeof(double), new TypeConverter<Color, double>(c => c.R)),
                    (typeof(int), new TypeConverter<Color, int>(c => c.R)),
                    (typeof(float), new TypeConverter<Color, float>(c => c.R)),
                ]
            },
            {
                typeof(Vec3D), [
                    (typeof(double), new TypeConverter<Vec3D, double>(v => v.X)),
                    (typeof(int), new TypeConverter<Vec3D, int>(v => (int)v.X)),
                    (typeof(VecD), new TypeConverter<Vec3D, VecD>(v => new VecD(v.X, v.Y))),
                    (typeof(VecI), new TypeConverter<Vec3D, VecI>(v => new VecI((int)v.X, (int)v.Y))),
                    (typeof(Color),
                        new TypeConverter<Vec3D, Color>(v => new Color((byte)Math.Clamp(v.X, 0, 255),
                            (byte)Math.Clamp(v.Y, 0, 255), (byte)Math.Clamp(v.Z, 0, 255)))),
                ]
            },
            {
                typeof(Texture), [
                    (typeof(Paintable), new TypeConverter<Texture, Paintable>(img => new TexturePaintable(img))),
                    (typeof(Painter),
                        new TypeConverter<Texture, Painter>(img =>
                            new Painter((c, s) => s.DrawSurface(img.DrawingSurface, 0, 0)))),
                ]
            },
            {
                typeof(IBrush), [
                    (typeof(DocumentReference),
                        new TypeConverter<IBrush, DocumentReference>(b =>
                            new DocumentReference(b.FilePath, b.OutputNodeId, b.Document)))
                ]
            }
        };

    public static bool TryConvert(object? arg, Type targetType, out object result)
    {
        if (arg is null)
        {
            result = null;
            return false;
        }

        if(arg.GetType().IsAssignableTo(targetType))
        {
            result = arg;
            return true;
        }

        if (arg is Delegate func)
        {
            try
            {
                var actualArg = func.DynamicInvoke(FuncContext.NoContext);
                return TryConvert(actualArg, targetType, out result);
            }
            catch
            {
                result = null;
                return false;
            }
        }

        if (targetType.IsAssignableTo(typeof(Delegate)))
        {
            result = arg;
            return true;
        }


        if (targetType.IsEnum)
        {
            if (arg is string enumStr)
            {
                return Enum.TryParse(targetType, enumStr, true, out result);
            }

            if (TryConvert(arg, Enum.GetUnderlyingType(targetType), out var underlyingValue))
            {
                try
                {
                    result = Enum.ToObject(targetType, underlyingValue);
                    return true;
                }
                catch
                {
                    result = null;
                    return false;
                }
            }

            result = null;
            return false;
        }

        Type argType = arg.GetType();

        if (_conversionTable.TryGetValue(argType, out var converters))
        {
            foreach (var (outType, converter) in converters)
            {
                if (targetType == outType)
                {
                    result = converter.Convert(arg);
                    return true;
                }
            }
        }

        if (!argType.IsPrimitive && argType != typeof(string))
        {
            var baseType = arg.GetType().BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType == targetType)
                {
                    result = arg;
                    return true;
                }

                if (_conversionTable.TryGetValue(baseType, out converters))
                {
                    foreach (var (outType, converter) in converters)
                    {
                        if (targetType == outType)
                        {
                            result = converter.Convert(arg);
                            return true;
                        }
                    }
                }

                baseType = baseType.BaseType;
            }

            var availableInterfaces = argType.GetInterfaces();
            foreach (var iface in availableInterfaces)
            {
                if (_conversionTable.TryGetValue(iface, out converters))
                {
                    foreach (var (outType, converter) in converters)
                    {
                        if (targetType == outType)
                        {
                            result = converter.Convert(arg);
                            return true;
                        }
                    }
                }
            }
        }

        try
        {
            if (!arg.GetType().IsValueType && targetType.IsValueType)
            {
                result = null;
                return false;
            }

            result = System.Convert.ChangeType(arg, targetType);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static object Convert(object arg, Type targetType)
    {
        if (TryConvert(arg, targetType, out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Cannot convert {arg.GetType()} to {targetType}");
    }

    private static int DoubleToInt(double d)
    {
        return (int)d;
    }

    private static double ConvertIntToDouble(int i)
    {
        return i;
    }

    private static double VecDToDouble(VecD vec)
    {
        return vec.X;
    }

    private static double VecIToDouble(VecI vecI)
    {
        return vecI.X;
    }

    private static VecD DoubleToVecD(double d)
    {
        return new VecD(d, d);
    }

    private static VecI DoubleToVecI(double d)
    {
        return new VecI((int)d, (int)d);
    }

    private static VecI IntToVecI(int i)
    {
        return new VecI(i, i);
    }

    private static VecD IntToVecD(int i)
    {
        return new VecD(i, i);
    }

    private static int VecIToInt(VecI vec)
    {
        return vec.X;
    }

    private static VecD VecIToVecD(VecI vec)
    {
        return new VecD(vec.X, vec.Y);
    }

    private static VecI VecDToVecI(VecD vec)
    {
        return new VecI((int)vec.X, (int)vec.Y);
    }

    private static int VecDToInt(VecD vec)
    {
        return (int)vec.X;
    }

    public static bool CanConvertType(Type inputValueType, Type outputValueType)
    {
        if (inputValueType == outputValueType)
        {
            return true;
        }

        if (_conversionTable.TryGetValue(outputValueType, out var converters))
        {
            foreach (var (outType, _) in converters)
            {
                if (outType == inputValueType)
                {
                    return true;
                }
            }
        }

        return outputValueType.IsAssignableFrom(inputValueType);
    }
}

interface ITypeConverter
{
    public object Convert(object arg);
}

class TypeConverter<TIn, TOut> : ITypeConverter
{
    private readonly Func<TIn, TOut> _converter;

    public TypeConverter(Func<TIn, TOut> converter)
    {
        _converter = converter;
    }

    public TOut Convert(TIn arg)
    {
        return _converter(arg);
    }

    object ITypeConverter.Convert(object arg)
    {
        return _converter((TIn)arg);
    }
}
