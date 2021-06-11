using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PixiEditor.Models.Tools
{
    public class ToolBuilder
    {
        private readonly IServiceProvider services;

        private readonly List<Type> toBuild = new List<Type>();

        public ToolBuilder(IServiceProvider services)
        {
            this.services = services;
        }

        public static T BuildTool<T>(IServiceProvider services)
            where T : Tool, new()
            => (T)BuildTool(typeof(T), services);

        public static Tool BuildTool(Type type, IServiceProvider services)
        {
            Tool tool = (Tool)type.GetConstructor(Type.EmptyTypes).Invoke(null);

            services.Inject(tool, BindingFlags.Public | BindingFlags.Instance, type);

            tool.SetupSubTools();

            return tool;
        }

        public ToolBuilder Add<T>()
            where T : Tool, new()
            => Add(typeof(T));

        public ToolBuilder Add(Type type)
        {
            toBuild.Add(type);

            return this;
        }

        public IEnumerable<Tool> Build()
        {
            List<Tool> tools = new List<Tool>();

            foreach (Type type in toBuild)
            {
                tools.Add(BuildTool(type, services));
            }

            return tools;
        }
    }
}
