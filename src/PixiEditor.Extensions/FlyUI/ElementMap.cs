using System.Reflection;
using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.FlyUI;

public class ElementMap
{
    public IReadOnlyDictionary<string, Type> ControlMap => controlMap;
    public IReadOnlyDictionary<Type, string> ControlMapReversed => controlMapReversed;
    public IReadOnlyDictionary<string, Type> WellKnownStructs => wellKnownStructs;

    // TODO: Code generation
    private Dictionary<string, Type> controlMap = new Dictionary<string, Type>();
    private Dictionary<Type, string> controlMapReversed = new Dictionary<Type, string>();
    private Dictionary<string, Type> wellKnownStructs = new Dictionary<string, Type>();

    public ElementMap()
    {

    }

    public void AddElementsFromAssembly(Assembly assembly)
    {
        var layoutElementTypes = assembly.GetTypes().Where(x => x is { IsAbstract: false, IsInterface: false } && x.GetInterfaces().Contains(typeof(ILayoutElement<Control>)));
        foreach (var type in layoutElementTypes)
        {
            controlMap.Add(type.Name, type); // TODO: Extension unique name prefix?
            if (!controlMapReversed.ContainsKey(type))
            {
                controlMapReversed.Add(type, type.Name);
            }
        }
        
        var structTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IStructProperty)));
        foreach (var type in structTypes)
        {
            wellKnownStructs.Add(type.Name, type);
        }
    }

    public byte[] Serialize()
    {
        // Dictionary format: [string controlTypeId, string controlTypeName]
        int size = controlMap.Count * (sizeof(int) + 1);
        List<byte> bytes = new List<byte>(size);

        int offset = 0;
        foreach (var (key, value) in controlMap)
        {
            bytes.AddRange(BitConverter.GetBytes(key.Length));
            offset += sizeof(int);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            offset++;
            byte[] nameBytes = Encoding.UTF8.GetBytes(value.Name);
            bytes.AddRange(BitConverter.GetBytes(nameBytes.Length));
            bytes.AddRange(nameBytes);
            offset += nameBytes.Length;
        }

        return bytes.ToArray();
    }

    public void Deserialize(byte[] bytes)
    {
        int offset = 0;
        while (offset < bytes.Length)
        {
            int controlTypeIdLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            string controlTypeId = Encoding.UTF8.GetString(bytes, offset, controlTypeIdLength);
            offset += controlTypeIdLength;

            int nameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            string name = Encoding.UTF8.GetString(bytes, offset, nameLength);
            offset += nameLength;
            controlMap.Add(controlTypeId, Type.GetType(name));
        }
    }
}
