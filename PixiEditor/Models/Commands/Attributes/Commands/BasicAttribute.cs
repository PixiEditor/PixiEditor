namespace PixiEditor.Models.Commands.Attributes;

public partial class Command
{
    public class BasicAttribute : CommandAttribute
    {
        /// <summary>
        /// Gets or sets the parameter that will be passed to the first argument of the method
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Create's a basic command which uses null as a paramter
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <param name="display">A short description which is displayed in the the top menu, e.g. "Save as..."</param>
        /// <param name="description">A description which is displayed in the search bar, e.g. "Save image as new". Leave empty to hide it from the search bar</param>
        public BasicAttribute(string name, string display, string description)
            : this(name, null, display, description)
        {
        }

        /// <summary>
        /// Create's a basic command which uses <paramref name="parameter"/> as the parameter
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <param name="parameter">The parameter that will be passed to the first argument of the method</param>
        /// <param name="display">A short description which is displayed in the the top menu, e.g. "Save as..."</param>
        /// <param name="description">A description which is displayed in the search bar, e.g. "Save image as new". Leave empty to hide it from the search bar</param>
        public BasicAttribute(string name, object parameter, string display, string description)
            : base(name, display, description)
        {
            Parameter = parameter;
        }
    }
}