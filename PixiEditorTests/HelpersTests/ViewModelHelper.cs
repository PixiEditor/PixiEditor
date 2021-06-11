using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.UserPreferences;
using PixiEditorTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditorTests.HelpersTests
{
    public static class ViewModelHelper
    {
        public static IServiceCollection GetViewModelMainCollection()
        {
            return new ServiceCollection()
                .AddScoped<IPreferences, PreferenceSettingsMock>()
                .AddSingleton<BitmapManager>();
        }
    }
}
