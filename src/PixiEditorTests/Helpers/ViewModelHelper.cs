using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.Main;
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
        //public static IServiceCollection GetViewModelMainCollection()
        //{
        //    return new ServiceCollection()
        //        .AddScoped<IPreferences, PreferenceSettingsMock>()
        //        .AddSingleton<BitmapManager>();
        //}

        //public static ViewModelMain MockedViewModelMain()
        //{
        //    IServiceCollection provider = MockedServiceCollection();

        //    return new ViewModelMain(provider);
        //}

        //public static IServiceCollection MockedServiceCollection()
        //{
        //    return new ServiceCollection()
        //        .AddSingleton<IPreferences>(new Mocks.PreferenceSettingsMock())
        //        .AddSingleton<StylusViewModel>()
        //        .AddSingleton<BitmapManager>()
        //        .AddSingleton<ToolsViewModel>();
        //}

        //public static T BuildMockedTool<T>(bool requireViewModelMain = false)
        //    where T : Tool
        //{
        //    IServiceProvider services;

        //    if (requireViewModelMain)
        //    {
        //        services = MockedViewModelMain().Services;
        //    }
        //    else
        //    {
        //        services = MockedServiceCollection().BuildServiceProvider();
        //    }

        //    return ToolBuilder.BuildTool<T>(services);
        //}
    }
}
