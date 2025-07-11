namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public static class InteropUtility
{
    public static byte[] SerializeToBytes(string[] list)
    {
        if (list == null || list.Length == 0)
        {
            return [];
        }

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(list.Length));
        foreach (var item in list)
        {
            if (item == null)
            {
                bytes.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                byte[] itemBytes = System.Text.Encoding.UTF8.GetBytes(item);
                bytes.AddRange(BitConverter.GetBytes(itemBytes.Length));
                bytes.AddRange(itemBytes);
            }
        }

        return bytes.ToArray();
    }
}
