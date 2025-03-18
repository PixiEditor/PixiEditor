using System.Numerics;
using System.Reflection;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public static class ConversionTable
{
    private static readonly Dictionary<Type, List<(Type, ITypeConverter)>> _conversionTable =
        new()
        {
            {
                typeof(double), [
                    (typeof(int), new TypeConverter<double, int>(DoubleToInt)),
                    (typeof(VecD), new TypeConverter<double, VecD>(DoubleToVecD)),
                    (typeof(VecI), new TypeConverter<double, VecI>(DoubleToVecI))
                ]
            },
            {
                typeof(int), [
                    (typeof(double), new TypeConverter<int, double>(ConvertIntToDouble)),
                    (typeof(VecI), new TypeConverter<int, VecI>(IntToVecI)),
                    (typeof(VecD), new TypeConverter<int, VecD>(IntToVecD)),
                ]
            },
            {
                typeof(VecD), [
                    (typeof(double), new TypeConverter<VecD, double>(VecDToDouble)),
                    (typeof(int), new TypeConverter<VecD, int>(VecDToInt)),
                    (typeof(VecI), new TypeConverter<VecD, VecI>(VecDToVecI)),
                ]
            },
            {
                typeof(VecI), [
                    (typeof(double), new TypeConverter<VecI, double>(VecIToDouble)),
                    (typeof(int), new TypeConverter<VecI, int>(VecIToInt)),
                    (typeof(VecD), new TypeConverter<VecI, VecD>(VecIToVecD))
                ]
            },
            {
                typeof(Color), [
                    (typeof(Paintable), new TypeConverter<Color, Paintable>(c => new ColorPaintable(c))),
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

        if (_conversionTable.TryGetValue(arg.GetType(), out var converters))
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

        try
        {
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
