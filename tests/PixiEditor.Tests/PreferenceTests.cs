using PixiEditor.Models.Commands;
using PixiEditor.Models.Preferences;
using PixiEditor.Tests.Helpers;

namespace PixiEditor.Tests;

public class PreferenceTests
{
    private const string ShortcutKey = "68";
    private const string ShortcutModifier = "0";
    private const string ShortcutCommand = "PixiEditor.Document.ClipCanvas";
    private const string ShortcutFile = $$$"""{"Shortcuts":[{"KeyCombination":{"Key":{{{ShortcutKey}}},"Modifiers":{{{ShortcutModifier}}}},"Commands":["{{{ShortcutCommand}}}"]}]}""";

    private const string UserPreferenceStringContent = "Hello World!";
    private const string UserPreferenceIntContent = "1";
    private const string UserPreferenceFile = $$"""{"{{nameof(UserPreferenceStringContent)}}":"{{UserPreferenceStringContent}}","{{nameof(UserPreferenceIntContent)}}":{{UserPreferenceIntContent}}}""";
    
    [Fact]
    public void TestThatNonExistentPreferenceFileWriteEmptyJson()
    {
        using var tempFile = DisposableFile.GetTemp();

        var preferences = new PreferencesSettings();
        preferences.Init(tempFile.Path, tempFile.Path);
        
        tempFile.AssertContent("{}");
    }
    
    [Fact]
    public void TestThatEmptyPreferenceFileDoesNotThrow()
    {
        using var tempFile = DisposableFile.CreateTemp();

        var preferences = new PreferencesSettings();
        preferences.Init(tempFile.Path, tempFile.Path);
        
        tempFile.AssertContent(string.Empty);
    }

    [Fact]
    public void TestThatExistingPreferencesGetParsed()
    {
        using var tempFile = DisposableFile.CreateTempWithContent(UserPreferenceFile);
        
        var preferences = new PreferencesSettings();
        preferences.Init(tempFile.Path, tempFile.Path);

        var localStringPreference = preferences.GetLocalPreference<string>(nameof(UserPreferenceStringContent));
        Assert.Equal(UserPreferenceStringContent, localStringPreference);
        
        var roamingStringPreference = preferences.GetPreference<string>(nameof(UserPreferenceStringContent));
        Assert.Equal(UserPreferenceStringContent, roamingStringPreference);

        var localIntPreference = preferences.GetLocalPreference<int>(nameof(UserPreferenceIntContent));
        Assert.Equal(int.Parse(UserPreferenceIntContent), localIntPreference);
        
        var roamingIntPreference = preferences.GetPreference<int>(nameof(UserPreferenceIntContent));
        Assert.Equal(int.Parse(UserPreferenceIntContent), roamingIntPreference);
    }

    [Fact]
    public void TestThatEmptyShortcutFileDoesNotThrow()
    {
        using var tempFile = DisposableFile.CreateTemp();

        var controller = new CommandController();
        var file = new ShortcutFile(tempFile.Path, controller);

        Assert.NotNull(file.LoadTemplate());
    }

    [Fact]
    public void TestThatExistingShortcutFileGetsParsed()
    {
        using var tempFile = DisposableFile.CreateTempWithContent(ShortcutFile);
        
        var controller = new CommandController();
        var file = new ShortcutFile(tempFile.Path, controller);

        var loadedTemplate = file.LoadTemplate();
        Assert.NotNull(loadedTemplate);
        
        Assert.Empty(loadedTemplate.Errors);
        Assert.NotEmpty(loadedTemplate.Shortcuts);
        
        Assert.Equal(int.Parse(ShortcutKey), (int)loadedTemplate.Shortcuts[0].KeyCombination.Key);
        Assert.Equal(int.Parse(ShortcutModifier), (int)loadedTemplate.Shortcuts[0].KeyCombination.Modifiers);
        
        Assert.Equal(ShortcutCommand, loadedTemplate.Shortcuts[0].Commands[0]);
    }
}
