using PixiEditor.Models.Commands;
using PixiEditor.Models.Preferences;
using PixiEditor.Tests.Helpers;

namespace PixiEditor.Tests;

public class PreferenceTests
{
    [Fact]
    public void TestNonExistentPreferenceFileWriteEmptyJson()
    {
        using var tempFile = DisposableFile.GetTemp();

        var preferences = new PreferencesSettings();
        preferences.Init(tempFile.Path, tempFile.Path);
        
        tempFile.AssertContent("{}");
    }
    
    [Fact]
    public void TestEmptyPreferenceFileDoesNotThrow()
    {
        using var tempFile = DisposableFile.CreateTemp();

        var preferences = new PreferencesSettings();
        preferences.Init(tempFile.Path, tempFile.Path);
        
        tempFile.AssertContent(string.Empty);
    }

    [Fact]
    public void TestEmptyShortcutFileDoesNotThrow()
    {
        using var tempFile = DisposableFile.CreateTemp();

        var controller = new CommandController();
        var file = new ShortcutFile(tempFile.Path, controller);

        Assert.NotNull(file.LoadTemplate());
    }
}
