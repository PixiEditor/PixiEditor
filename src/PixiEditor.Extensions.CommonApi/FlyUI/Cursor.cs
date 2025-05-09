using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.CommonApi.FlyUI;

public struct Cursor : IStructProperty
{
    private const int Version = 1; // Serialization version, increment when changing the struct
    public BuiltInCursor? BuiltInCursor { get; set; }
    public bool IsCustom => BuiltInCursor == null;

    public Cursor(BuiltInCursor builtInCursor)
    {
        BuiltInCursor = builtInCursor;
    }

    byte[] IStructProperty.Serialize()
    {
        var data = new List<byte>();
        data.Add(Version);
        data.Add(BuiltInCursor != null ? (byte)1 : (byte)0);
        if (BuiltInCursor != null)
        {
            data.Add((byte)BuiltInCursor.Value);
        }

        return data.ToArray();
    }

    void IStructProperty.Deserialize(byte[] data)
    {
        int version = data[0];

        int index = 1;
        if (data[index] == 1)
        {
            index++;
            BuiltInCursor = (BuiltInCursor)data[index];
        }
        else
        {
            BuiltInCursor = null;
        }
    }
}

public enum BuiltInCursor
{
    Arrow,
    IBeam,
    Wait,
    Cross,
    UpArrow,
    SizeWestEast,
    SizeNorthSouth,
    SizeAll,
    No,
    Hand,
    AppStarting,
    Help,
    TopSide,
    BottomSide,
    LeftSide,
    RightSide,
    TopLeftCorner,
    TopRightCorner,
    BottomLeftCorner,
    BottomRightCorner,
    DragMove,
    DragCopy,
    DragLink,
    None,
}
