using System.Reflection;
using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding;

public class ElementMap
{
    public IReadOnlyDictionary<int, Type> ControlMap => controlMap;

    // TODO: Code generation
    private Dictionary<int, Type> controlMap = new Dictionary<int, Type>();

    public ElementMap()
    {

    }

    public void AddElementsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ILayoutElement<Control>)));
        int id = controlMap.Count;
        foreach (var type in types)
        {
            controlMap.Add(id, type);
            id++;
        }
    }

    public byte[] Serialize()
    {
        // Dictionary format: [int bytes controlTypeId, string controlTypeName]
        int size = controlMap.Count * (sizeof(int) + 1);
        List<byte> bytes = new List<byte>(size);

        int offset = 0;
        foreach (var (key, value) in controlMap)
        {
            bytes.AddRange(BitConverter.GetBytes(key));
            offset++;
            byte[] nameBytes = Encoding.UTF8.GetBytes(value.Name);
            bytes.AddRange(BitConverter.GetBytes(nameBytes.Length));
            bytes.AddRange(nameBytes);
            offset += nameBytes.Length;
        }

        return bytes.ToArray();
    }
}
