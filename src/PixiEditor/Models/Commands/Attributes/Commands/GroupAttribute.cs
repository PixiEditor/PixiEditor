namespace PixiEditor.Models.Commands.Attributes;

internal partial class Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class GroupAttribute : Attribute
    {
        public string InternalName { get; }

        public string DisplayName { get; }

        /// <summary>
        /// Groups all commands that start with the name <paramref name="internalName"/>
        /// </summary>
        public GroupAttribute(string internalName, string displayName)
        {
            InternalName = internalName;
            DisplayName = displayName;
        }
    }
}
