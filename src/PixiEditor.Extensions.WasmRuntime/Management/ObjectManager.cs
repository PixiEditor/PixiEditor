namespace PixiEditor.Extensions.WasmRuntime.Management;

internal class ObjectManager
{
    private Dictionary<int, object> ManagedObjects { get; } = new Dictionary<int, object>();

    public int AddObject(object obj)
    {
        int id = ManagedObjects.Count + 1; // 0 is reserved for null
        ManagedObjects.Add(id, obj);
        return id;
    }

    public T GetObject<T>(int id)
    {
        object obj = ManagedObjects[id];
        if (obj is not T)
        {
            throw new InvalidCastException($"Object with id {id} is not of type {typeof(T).Name}");
        }

        return (T)obj;
    }

    public void RemoveObject(int id)
    {
        ManagedObjects.Remove(id);
    }
}
