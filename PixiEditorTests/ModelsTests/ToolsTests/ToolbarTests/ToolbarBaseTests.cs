using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests.ToolbarTests
{
    [Collection("Application collection")]
    public class ToolbarBaseTests
    {
        [StaFact]
        public void TestThatGetSettingReturnsCorrectSetting()
        {
            BasicToolbar toolbar = new BasicToolbar();
            string settingName = "ToolSize";

            Setting setting = toolbar.GetSetting(settingName);

            Assert.NotNull(setting);
            Assert.Equal(settingName,setting.Name);
        }

        [StaFact]
        public void TestThatGenericGetSettingReturnsSettingWithCorrectType()
        {
            const string settingName = "test";
            const bool settingValue = true;
            Setting<bool> expected = new BoolSetting(settingName, settingValue);

            BasicToolbar toolbar = new BasicToolbar();
            toolbar.Settings.Add(expected);

            Setting<bool> actual = toolbar.GetSetting<bool>(settingName);

            Assert.Equal(expected.Value, actual.Value);
        }

        [StaFact]
        public void TestThatGenericGetSettingReturnsNullWhenSettingIsNotFound()
        {
            BasicToolbar toolbar = new BasicToolbar();

            Setting<bool> actual = toolbar.GetSetting<bool>("invalid");

            Assert.Null(actual);
        }

        [StaFact]
        public void TestThatGenericGetSettingReturnsNullWhenSettingHasValueWithWrongType()
        {
            const string settingName = "test";
            BasicToolbar toolbar = new BasicToolbar();
            toolbar.Settings.Add(new BoolSetting(settingName));

            Setting<int> actual = toolbar.GetSetting<int>(settingName);

            Assert.Null(actual);
        }

        [StaFact]
        public void TestThatSaveToolbarSettingsSavesSettingAndLoadsItIntoNewToolbar()
        {
            BasicToolbar toolbar = new BasicToolbar();

            toolbar.GetSetting<int>("ToolSize").Value = 5;

            toolbar.SaveToolbarSettings();

            BasicShapeToolbar shapeToolbar = new BasicShapeToolbar();

            Assert.NotEqual(5, shapeToolbar.GetSetting<int>("ToolSize").Value);

            shapeToolbar.LoadSharedSettings();

            Assert.Equal(5, shapeToolbar.GetSetting<int>("ToolSize").Value);
        }
    }
}
