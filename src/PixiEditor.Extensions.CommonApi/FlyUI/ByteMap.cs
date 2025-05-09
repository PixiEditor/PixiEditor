namespace PixiEditor.Extensions.CommonApi.FlyUI;

public static class ByteMap
{
    public static byte GetTypeByteId(Type type)
    {
        if (type == null)
        {
            return 255;
        }
        if (type == typeof(int))
        {
            return 0;
        }
        if (type == typeof(float))
        {
            return 1;
        }
        if (type == typeof(bool))
        {
            return 2;
        }
        if (type == typeof(double))
        {
            return 3;
        }
        if (type == typeof(long))
        {
            return 4;
        }
        if (type == typeof(short))
        {
            return 5;
        }
        if (type == typeof(byte))
        {
            return 6;
        }
        if (type == typeof(char))
        {
            return 7;
        }
        if (type == typeof(string))
        {
            return 8;
        }
        if(type == typeof(byte[]))
        {
            return 9;
        }

        throw new Exception($"Unknown unmanaged type: {type}");
    }

    public static Type GetTypeFromByteId(byte id)
    {
        return id switch
        {
            0 => typeof(int),
            1 => typeof(float),
            2 => typeof(bool),
            3 => typeof(double),
            4 => typeof(long),
            5 => typeof(short),
            6 => typeof(byte),
            7 => typeof(char),
            8 => typeof(string),
            9 => typeof(byte[]),
            255 => null,
            _ => throw new Exception($"Unknown unmanaged type id: {id}")
        };
    }
}
