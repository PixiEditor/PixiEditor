using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal class OptionsDialog<T> : CustomDialog, IEnumerable<T> 
    where T : notnull
{
    private Dictionary<T, Action<T>?> _results = new();

    public string Title { get; set; }

    public object Content { get; set; }

    public T? Result { get; private set; }

    public OptionsDialog(LocalizedString title, object content, Window ownerWindow) : base(ownerWindow)
    {
        Title = title;

        if (content is not Visual)
        {
            Content = new TextBlock()
            {
                Text = content.ToString(),
                FontSize = 15,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.WordEllipsis,
                TextWrapping = TextWrapping.WrapWithOverflow,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        else
        {
            Content = content;
        }
    }

    public OptionsDialog(string title, object content, IEnumerable<KeyValuePair<T, Action<T>>> options, Window ownerWindow) 
        : this(title, content, ownerWindow)
    {
        _results = new(options);
    }

    public Action<T>? this[T name]
    {
        get => _results[name];
        set => _results.Add(name, value);
    }

    public override Task<bool> ShowDialog() => ShowDialog(false);

    public async Task<bool> ShowDialog(bool topmost)
    {
        var popup = new OptionPopup(Title, Content, new(_results.Keys.Select(x => (object)x)));
        popup.Topmost = topmost;
        await popup.ShowDialog(OwnerWindow);

        Result = (T?)popup.Result;
        if (Result != null)
        {
            _results[Result]?.Invoke(Result);
        }

        return Result is not null;
    }

    public void Add(T name) => _results.Add(name, null);

    public void Add(T name, Action<T> action) => _results.Add(name, action);

    public IEnumerator<T> GetEnumerator() => _results.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _results.Keys.GetEnumerator();
}
