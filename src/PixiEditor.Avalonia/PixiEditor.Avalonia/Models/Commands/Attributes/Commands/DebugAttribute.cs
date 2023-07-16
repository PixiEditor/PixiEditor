namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    internal class DebugAttribute : BasicAttribute
    {
        public DebugAttribute([InternalName] string internalName, string displayName, string descriptiveName) : base($"#DEBUG#{internalName}", displayName, descriptiveName)
        {
        }

        public DebugAttribute([InternalName] string internalName, object parameter, string displayName, string description)
            : base($"#DEBUG#{internalName}", parameter, displayName, description)
        {
        }
    }
}
