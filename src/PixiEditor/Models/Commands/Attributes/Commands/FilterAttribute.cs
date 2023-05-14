using PixiEditor.Models.Localization;

namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    public class FilterAttribute : CommandAttribute
    {
        public LocalizedString SearchTerm { get; }
        
        public FilterAttribute(string internalName, string displayName, string searchTerm) : base(internalName, displayName, string.Empty)
        {
            SearchTerm = searchTerm;
        }
        
        public FilterAttribute(string internalName) : base(internalName, null, null) { }
    }
}
