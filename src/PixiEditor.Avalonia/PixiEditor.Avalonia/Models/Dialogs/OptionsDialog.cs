using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Models.Dialogs;

internal class OptionsDialog<T> : CustomDialog, IEnumerable<T>
{
    private Dictionary<T, Action<T>> _results = new();

    public string Title { get; set; }

    public object Content { get; set; }

    public T Result { get; private set; }

    public OptionsDialog(LocalizedString title, object content)
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

    public OptionsDialog(string title, object content, IEnumerable<KeyValuePair<T, Action<T>>> options) : this(title, content)
    {
        _results = new(options);
    }

    public Action<T> this[T name]
    {
        get => _results[name];
        set => _results.Add(name, value);
    }

    public override async Task<bool> ShowDialog()
    {
        //TODO: Implement
        /*var popup = new OptionPopup(Title, Content, new(_results.Keys.Select(x => (object)x)));
        var popupResult = popup.ShowDialog();

        Result = (T)popup.Result;
        if (Result != null)
        {
            _results[Result]?.Invoke(Result);
        }

        return popupResult.GetValueOrDefault(false);*/
        return false;
    }

    public void Add(T name) => _results.Add(name, null);

    public void Add(T name, Action<T> action) => _results.Add(name, action);

    public IEnumerator<T> GetEnumerator() => _results.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _results.Keys.GetEnumerator();
}
