namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public static class LayoutElementIdGenerator
{
    private static int _lastId = -1;

    public static int GetNextId()
    {
        _lastId++;
        return _lastId;
    }
}
