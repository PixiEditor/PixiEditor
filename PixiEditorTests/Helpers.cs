using System;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;

namespace PixiEditorTests
{
    public static class Helpers
    {
        public static ViewModelMain MockedViewModelMain()
        {
            IServiceProvider provider = MockedServiceProvider();

            return new ViewModelMain(provider);
        }

        public static IServiceProvider MockedServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<IPreferences>(new Mocks.PreferenceSettingsMock())
                .BuildServiceProvider();
        }
    }
}