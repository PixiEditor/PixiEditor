namespace PixiEditor.Extensions.CommonApi.FlyUI;

public static class LayoutElementIdGenerator
{
    private static int _lastId = -1;

    public static int GetNextId()
    {
        _lastId++;
        return _lastId;
    }
}
