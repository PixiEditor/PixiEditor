using System.Collections;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public class TextEventArgs : ElementEventArgs<TextEventArgs>
{
    public string Text { get; set; }

    public TextEventArgs(string newText)
    {
        Text = newText;
    }

    protected override void SerializeArgs(ByteWriter writer)
    {
        writer.WriteString(Text);
    }
}
