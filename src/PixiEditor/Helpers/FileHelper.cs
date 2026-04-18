namespace PixiEditor.Helpers;

public static class FileHelper
{
    public static string GetUniqueFileName(string fileName)
    {
        string directory = Path.GetDirectoryName(fileName) ?? "";
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        int counter = 1;
        string uniqueFileName = fileName;

        while (File.Exists(uniqueFileName))
        {
            uniqueFileName = Path.Combine(directory, $"{nameWithoutExtension}({counter}){extension}");
            counter++;
        }

        return uniqueFileName;
    }
}
