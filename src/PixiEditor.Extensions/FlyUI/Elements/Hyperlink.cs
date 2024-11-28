using System.Collections.Immutable;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Hyperlink : StatelessElement, IPropertyDeserializable
{
    public string Url { get; set; }
    public string Text { get; set; }
    
    public Hyperlink(string url, string text)
    {
        Url = url;
        Text = text;
    }

    public override Control BuildNative()
    {
        Hyperlink
        
       
        return hyperlink;
    }

    public IEnumerable<object> GetProperties()
    {
        throw new NotImplementedException();
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        throw new NotImplementedException();
    }
}
