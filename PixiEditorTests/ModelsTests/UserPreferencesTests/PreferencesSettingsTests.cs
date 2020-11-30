using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PixiEditor.Models.UserPreferences;
using Xunit;

namespace PixiEditorTests.ModelsTests.UserPreferencesTests
{
    public class PreferencesSettingsTests
    {
        public static string PathToPreferencesFile { get; } = Path.Join("PixiEditor", "test_preferences.json");

        public PreferencesSettingsTests()
        {
            PreferencesSettings.Init(PathToPreferencesFile);
        }

        [Fact]
        public void TestThatPreferencesSettingsIsLoaded()
        {
            Assert.True(PreferencesSettings.IsLoaded);
        }

        [Fact]
        public void TestThatInitCreatesUserPreferencesJson()
        {
            Assert.True(File.Exists(PathToPreferencesFile));
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(false)]
        [InlineData("string")]
        [InlineData(null)]
        public void TestThatGetPreferenceOnNonExistingKeyReturnsFallbackValue<T>(T value)
        {
            T fallbackValue = value;
            T preferenceValue = PreferencesSettings.GetPreference<T>("NonExistingPreference", fallbackValue);
            Assert.Equal(fallbackValue, preferenceValue);
        }

        [Theory]
        [InlineData("IntPreference", 1)]
        [InlineData("BoolPreference", true)]
        public void TestThatUpdatePreferenceUpdatesDictionary<T>(string name, T value)
        {
            PreferencesSettings.UpdatePreference(name, value);
            Assert.Equal(value, PreferencesSettings.GetPreference<T>(name));
        }

        [Theory]
        [InlineData("LongPreference", 1L)]
        public void TestThatSaveUpdatesFile<T>(string name, T value)
        {
            PreferencesSettings.Preferences[name] = value;
            PreferencesSettings.Save();
            using (var fs = new FileStream(PathToPreferencesFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using StreamReader sr = new StreamReader(fs);
                string json = sr.ReadToEnd();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Assert.True(dict.ContainsKey(name));
                Assert.Equal(value, dict[name]);
            }
        }
    }
}