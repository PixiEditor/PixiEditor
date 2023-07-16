namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    /// <summary>
    /// A command that is not shown in the UI
    /// </summary>
    internal class InternalAttribute : BasicAttribute
    {
        /// <summary>
        /// A command that is not shown in the UI
        /// </summary>
        public InternalAttribute([InternalName] string name)
            : base(name, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// A command that is not shown in the UI
        /// </summary>
        public InternalAttribute([InternalName] string name, object parameter)
            : base(name, parameter, string.Empty, string.Empty)
        {
        }
    }
}
