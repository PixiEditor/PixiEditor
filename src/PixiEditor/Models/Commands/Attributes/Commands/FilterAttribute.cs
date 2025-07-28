using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    public class FilterAttribute : CommandAttribute
    {
        public LocalizedString SearchTerm { get; }
        
        public FilterAttribute([InternalName] string internalName, string displayName, string searchTerm) : base(internalName, displayName, string.Empty)
        {
            SearchTerm = searchTerm;
        }
        
        public FilterAttribute([InternalName] string internalName) : base(internalName, null, null) { }
    }
}
