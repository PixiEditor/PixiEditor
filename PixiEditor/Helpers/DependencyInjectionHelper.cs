using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PixiEditor.Helpers
{
    public static class DependencyInjectionHelper
    {
        public static T Inject<T>(this IServiceProvider provider)
            => (T)Inject(provider, typeof(T));

#nullable enable

        public static object Inject(this IServiceProvider provider, Type type)
        {
            ConstructorInfo constructor = FindConstructorOrDefault(provider, type);

            List<object?> parameters = new List<object?>();

            foreach (Type argumentType in constructor.GetParameters().Select(x => x.ParameterType))
            {
                parameters.Add(provider.GetRequiredService(argumentType));
            }

            return constructor.Invoke(parameters.ToArray());
        }

#nullable disable

        private static ConstructorInfo FindConstructorOrDefault(IServiceProvider provider, Type type)
        {
            ConstructorInfo foundConstructor = default;

            foreach (ConstructorInfo info in type.GetConstructors())
            {
                if (HasParameters(provider, info.GetParameters()))
                {
                    foundConstructor = info;
                    break;
                }
            }

            return foundConstructor;
        }

        private static bool HasParameters(IServiceProvider provider, IEnumerable<ParameterInfo> parameters)
        {
            foreach (ParameterInfo parameter in parameters)
            {
                if (provider.GetService(parameter.ParameterType) is null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
