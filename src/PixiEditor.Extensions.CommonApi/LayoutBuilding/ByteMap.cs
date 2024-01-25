namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public static class ByteMap
{
    public static IReadOnlyDictionary<string, int> ControlMap => controlMap;

    private static Dictionary<string, int> controlMap = new Dictionary<string, int>()
    {
        // If you do add a new control, make sure to add it to GlobalControlFactory inside PixiEditor project
        { "Layout", 0 },
        { "Center", 1 },
        { "Text", 2 }
    };

    public static byte GetTypeByteId(Type type)
    {
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
            _ => throw new Exception($"Unknown unmanaged type id: {id}")
        };
    }
}
