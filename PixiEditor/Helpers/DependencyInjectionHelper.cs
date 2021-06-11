using System;
using System.Reflection;

namespace PixiEditor.Helpers
{
    public static class DependencyInjectionHelper
    {
        /// <summary>
        /// Injects all services from <paramref name="services"/> into the public properties of <paramref name="obj"/>
        /// </summary>
        /// <typeparam name="T">The type of the object to inject</typeparam>
        /// <param name="services">The <see cref="IServiceProvider"/></param>
        /// <param name="obj">The object that should get injected</param>
        public static void Inject<T>(this IServiceProvider services, T obj)
            => Inject(services, obj, BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Injects all services from <paramref name="services"/> into the properties of <paramref name="obj"/>
        /// </summary>
        /// <typeparam name="T">The type of the object to inject</typeparam>
        /// <param name="services">The <see cref="IServiceProvider"/></param>
        /// <param name="obj">The object that should get injected</param>
        /// <param name="bindingFlags">The binding flags for the properties</param>
        public static void Inject<T>(this IServiceProvider services, T obj, BindingFlags bindingFlags)
            => Inject(services, obj, bindingFlags, typeof(T));

        public static void Inject(this IServiceProvider services, object obj, BindingFlags bindingFlags, Type type)
        {
            foreach (PropertyInfo info in type.GetProperties(bindingFlags))
            {
                if (!info.CanWrite)
                {
                    continue;
                }

                object value = services.GetService(info.PropertyType);

                if (value is null)
                {
                    continue;
                }

                info.SetValue(obj, value);
            }
        }
    }
}
