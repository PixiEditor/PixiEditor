namespace PixiEditor.Models.Commands.Attributes
{
    public partial class Command
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class GroupAttribute : Attribute
        {
            public string Name { get; }

            public string Display { get; }

            /// <summary>
            /// Group's all commands that start with the name <paramref name="name"/>
            /// </summary>
            public GroupAttribute(string name, string display)
            {
                Name = name;
                Display = display;
            }
        }
    }
}
