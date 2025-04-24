using System.Text;
using System.Text.Json;

namespace PixiEditor.OperatingSystem;

public static class SecureStorage
{
    public static string PathToStorage => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixiEditor",
        "SecureStorage.data");

    static SecureStorage()
    {
        if (!File.Exists(PathToStorage))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToStorage)!);
            File.Create(PathToStorage).Dispose();
        }
    }

    public static async Task<T?> GetValueAsync<T>(string key, T? defaultValue = default)
    {
        byte[] current = ReadExistingData();

        if (current is { Length: > 0 })
        {
            byte[] decryptedData = IOperatingSystem.Current.Encryptor.Decrypt(current);

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

    public static async Task SetValueAsync<T>(string key, T value)
    {
        byte[] current = ReadExistingData();

        Dictionary<string, object> data = new Dictionary<string, object>();

        if (current is { Length: > 0 })
        {
            byte[] decryptedData = IOperatingSystem.Current.Encryptor.Decrypt(current);

            string existingValue = Encoding.UTF8.GetString(decryptedData);
            data = JsonSerializer.Deserialize<Dictionary<string, object>>(existingValue) ??
                   new Dictionary<string, object>();
        }

        data[key] = value;

        byte[] newData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        byte[] encryptedData = IOperatingSystem.Current.Encryptor.Encrypt(newData);

        await using var stream = new FileStream(PathToStorage, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
        await stream.FlushAsync();
    }

    private static byte[] ReadExistingData()
    {
        var stream = new FileStream(PathToStorage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] existingData = new byte[stream.Length];
        stream.ReadExactly(existingData, 0, existingData.Length);
        stream.Close();
        return existingData;
    }

    public static T GetValue<T>(string key, T? defaultValue = default)
    {
        byte[] current = ReadExistingData();

        if (current is { Length: > 0 })
        {
            byte[] decryptedData = IOperatingSystem.Current.Encryptor.Decrypt(current);

            string existingValue = Encoding.UTF8.GetString(decryptedData);
            try
            {
                Dictionary<string, object>?
                    data = JsonSerializer.Deserialize<Dictionary<string, object>>(existingValue);
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
            catch (JsonException)
            {
               return defaultValue;
            }
        }

        return defaultValue;
    }
}
