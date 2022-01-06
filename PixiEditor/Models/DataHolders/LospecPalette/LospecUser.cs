namespace PixiEditor.Models.DataHolders.LospecPalette
{
    public class LospecUser
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Url => $"https://lospec.com/{Slug}";
    }
}
