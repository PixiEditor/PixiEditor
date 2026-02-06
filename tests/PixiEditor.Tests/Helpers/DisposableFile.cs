namespace PixiEditor.Tests.Helpers;

public sealed class DisposableFile(string path) : IDisposable
{
    public string Path { get; } = path;
    
    /// <summary>
    /// Creates a disposable temporary file with a unique name.
    /// There will already be an empty file at the path.
    /// Use <see cref="GetTemp"/> to get a created file.
    /// </summary>
    public static DisposableFile CreateTemp() =>
        new(System.IO.Path.GetTempFileName());

    /// <summary>
    /// Gets a disposable temporary file with a unique name.
    /// There will be no file at the path.
    /// Use <see cref="CreateTemp"/> to get a created file.
    /// </summary>
    public static DisposableFile GetTemp()
    {
        var file = CreateTemp();
        
        File.Delete(file.Path);
        
        return file;
    }
    
    public static DisposableFile CreateTempWithContent(string content)
    {
        var file = CreateTemp();
        
        File.WriteAllText(file.Path, content);
        
        return file;
    }

    public FileStream Open(FileMode mode, FileAccess access) => File.Open(Path, mode, access);
    
    public string ReadAllText() => File.ReadAllText(Path);
    
    public void AssertContent(string expected) => Assert.Equal(expected, File.ReadAllText(Path));
    
    public void Dispose() => File.Delete(Path);
}
