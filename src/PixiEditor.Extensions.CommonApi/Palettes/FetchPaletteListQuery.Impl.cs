using ProtoBuf;

namespace PixiEditor.Extensions.CommonApi.Palettes;

public partial class FetchPaletteListQuery
{
    public FetchPaletteListQuery()
    {
    }
    
    public FetchPaletteListQuery(string dataSourceName, int startIndex, int items, FilteringSettings filtering)
    {
        DataSourceName = dataSourceName;
        StartIndex = startIndex;
        Items = items;
        Filtering = filtering;
    }
    
    public override string ToString()
    {
        return $"DataSourceName: {DataSourceName}, StartIndex: {StartIndex}, Items: {Items}, Filtering: {Filtering}";
    }
}
