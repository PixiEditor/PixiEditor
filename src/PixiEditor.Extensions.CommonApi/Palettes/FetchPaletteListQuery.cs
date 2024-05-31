using ProtoBuf;

namespace PixiEditor.Extensions.CommonApi.Palettes;

[ProtoContract]
public class FetchPaletteListQuery
{
    [ProtoMember(1)] 
    public string DataSourceName { get; set; }

    [ProtoMember(2)] 
    public int StartIndex { get; set; }

    [ProtoMember(3)] 
    public int Items { get; set; }

    [ProtoMember(4)] 
    public FilteringSettings Filtering { get; set; }

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
