namespace PixiEditor.Models.Commands.Attributes
{
    public partial class Command
    {
        /// <summary>
        /// A command that is not shown in the UI
        /// </summary>
        public class InternalAttribute : BasicAttribute
        {
            /// <summary>
            /// A command that is not shown in the UI
            /// </summary>
            public InternalAttribute(string name)
                : base(name, null, null)
            {
            }

            /// <summary>
            /// A command that is not shown in the UI
            /// </summary>
            public InternalAttribute(string name, object parameter)
                : base(name, parameter, null, null)
            {
            }
        }
    }
}
