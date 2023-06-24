namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    internal class BasicAttribute : CommandAttribute
    {
        /// <summary>
        /// Gets or sets the parameter that will be passed to the first argument of the method
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Create's a basic command which uses null as a paramter
        /// </summary>
        /// <param name="internalName">The internal name of the command</param>
        /// <param name="displayName">A short description which is displayed in the the top menu, e.g. "Save as...". Accepts localized key</param>
        /// <param name="descriptiveName">A description which is displayed in the search bar, e.g. "Save image as new". Leave empty to hide it from the search bar. Accepts localized key</param>
        public BasicAttribute([InternalName] string internalName, string displayName, string descriptiveName)
            : this(internalName, null, displayName, descriptiveName)
        {
        }

        /// <summary>
        /// Create's a basic command which uses <paramref name="parameter"/> as the parameter
        /// </summary>
        /// <param name="internalName">The internal name of the command</param>
        /// <param name="parameter">The parameter that will be passed to the first argument of the method</param>
        /// <param name="displayName">A short description which is displayed in the the top menu, e.g. "Save as...". Accepts localized key</param>
        /// <param name="description">A description which is displayed in the search bar, e.g. "Save image as new". Leave empty to hide it from the search bar. Accepts localized key</param>
        public BasicAttribute([InternalName] string internalName, object parameter, string displayName, string description)
            : base(internalName, displayName, description)
        {
            Parameter = parameter;
        }
    }
}
