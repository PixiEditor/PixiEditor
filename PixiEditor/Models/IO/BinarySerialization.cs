using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PixiEditor.Models.IO
{
    public static class BinarySerialization
    {
        public static void WriteToBinaryFile<T>(string path, T objectToWrite)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        public static T ReadFromBinaryFile<T>(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
