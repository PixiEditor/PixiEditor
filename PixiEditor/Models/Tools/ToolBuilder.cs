using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PixiEditor.Models.Tools
{
    /// <summary>
    /// Handles Depdency Injection of tools
    /// </summary>
    public class ToolBuilder
    {
        private readonly IServiceProvider services;

        private readonly List<Type> toBuild = new List<Type>();

        public ToolBuilder(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Constructs a new tool of type <typeparamref name="T"/> and injects all services of <paramref name="services"/>
        /// </summary>
        public static T BuildTool<T>(IServiceProvider services)
            where T : Tool, new()
            => (T)BuildTool(typeof(T), services);

        /// <summary>
        /// Constructs a new tool of type <paramref name="type"/> and injects all services of <paramref name="services"/>
        /// </summary>
        public static Tool BuildTool(Type type, IServiceProvider services)
        {
            Tool tool = (Tool)services.Inject(type);

            return tool;
        }

        /// <summary>
        /// Adds a new tool of type <typeparamref name="T"/> to the building chain.
        /// </summary>
        public ToolBuilder Add<T>()
            where T : Tool
            => Add(typeof(T));

        /// <summary>
        /// Adds a new tool of type <paramref name="type"/> to the building chain.
        /// </summary>
        public ToolBuilder Add(Type type)
        {
            toBuild.Add(type);

            return this;
        }

        /// <summary>
        /// Builds all added tools.
        /// </summary>
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
