namespace PixiEditor.Models.Commands.Attributes.Commands;

internal partial class Command
{
    public class FilterAttribute : CommandAttribute
    {
        public string SearchTerm { get; }
        
        public FilterAttribute(string internalName, string displayName, string searchTerm) : base(internalName, displayName, string.Empty)
        {
            SearchTerm = searchTerm;
        }
    }
}
