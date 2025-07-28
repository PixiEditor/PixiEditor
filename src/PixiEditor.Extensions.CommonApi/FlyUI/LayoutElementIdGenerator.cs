namespace PixiEditor.Extensions.CommonApi.FlyUI;

public static class LayoutElementIdGenerator
{
    private static int _lastId = -1;
    
    public static int CurrentId => _lastId;

    public static int GetNextId()
    {
        _lastId++;
        return _lastId;
    }

    public static void SetId(int id)
    {
        if (id > _lastId)
        {
            _lastId = id;
        }
    }
}
