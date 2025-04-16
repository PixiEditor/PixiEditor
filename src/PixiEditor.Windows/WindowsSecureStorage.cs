using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

internal class WindowsSecureStorage : ISecureStorage
{
    public string PathToStorage => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixiEditor",
        "SecureStorage.data");

    public WindowsSecureStorage()
    {
        if (!File.Exists(PathToStorage))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToStorage)!);
            File.Create(PathToStorage).Dispose();
        }
    }

    public async Task<T?> GetValueAsync<T>(string key, T defaultValue = default)
    {
        byte[] current = ReadExistingData();

        if (current is { Length: > 0 })
        {
            byte[] decryptedData = ProtectedData.Unprotect(
                current,
                null,
                DataProtectionScope.CurrentUser);

            string existingValue = Encoding.UTF8.GetString(decryptedData);
            Dictionary<string, object>? data = JsonSerializer.Deserialize<Dictionary<string, object>>(existingValue);
            if (data != null && data.TryGetValue(key, out object value))
            {
                if (value is JsonElement jsonElement)
                {
                    string jsonString = jsonElement.GetRawText();
                    return JsonSerializer.Deserialize<T>(jsonString);
                }

                return (T)value;
            }
        }

        return defaultValue;
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        byte[] current = ReadExistingData();

        Dictionary<string, object> data = new Dictionary<string, object>();

        if(current is { Length: > 0 })
        {
            byte[] decryptedData = ProtectedData.Unprotect(
                current,
                null,
                DataProtectionScope.CurrentUser);

            string existingValue = Encoding.UTF8.GetString(decryptedData);
            data = JsonSerializer.Deserialize<Dictionary<string, object>>(existingValue) ?? new Dictionary<string, object>();
        }

        data[key] = value;

        byte[] newData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        byte[] encryptedData = ProtectedData.Protect(
            newData,
            null,
            DataProtectionScope.CurrentUser);

        await using var stream = new FileStream(PathToStorage, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
        await stream.FlushAsync();
    }

    private byte[] ReadExistingData()
    {
        var stream = new FileStream(PathToStorage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] existingData = new byte[stream.Length];
        stream.ReadExactly(existingData, 0, existingData.Length);
        stream.Close();
        return existingData;
    }
}
