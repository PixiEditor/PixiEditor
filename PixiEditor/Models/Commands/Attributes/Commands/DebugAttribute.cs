namespace PixiEditor.Models.Commands.Attributes
{
    public partial class Command
    {
        public class DebugAttribute : BasicAttribute
        {
            public DebugAttribute(string name, string display, string description) : base($"#DEBUG#{name}", display, description)
            {
            }

            public DebugAttribute(string name, object parameter, string display, string description)
                : base($"#DEBUG#{name}", parameter, display, description)
            {
            }
        }
    }
}
