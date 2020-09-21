using System;
using System.Collections.Generic;
using System.Text;
using PixiEditor.Models.Tools.ToolSettings;
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

            Setting setting = toolbar.GetSetting<int>(settingName);

            Assert.NotNull(setting);
            Assert.Equal(settingName,setting.Name);
        }

        [StaFact]
        public void TestThatSaveToolbarSettingsSavesSettingAndLoadsItIntoNewToolbar()
        {
            BasicToolbar toolbar = new BasicToolbar();

            toolbar.Settings[0].Value = 5;

            toolbar.SaveToolbarSettings();

            BasicShapeToolbar shapeToolbar = new BasicShapeToolbar();

            Assert.NotEqual(5, shapeToolbar.GetSetting<int>("ToolSize").Value);

            shapeToolbar.LoadSharedSettings();

            Assert.Equal(5, shapeToolbar.GetSetting<int>("ToolSize").Value);
        }
    }
}
