using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Collections;

public class ActionDisplayList : IEnumerable<KeyValuePair<string, LocalizedString>>
{
    private Dictionary<string, LocalizedString> _dictionary = new();
    private Action notifyUpdate;

    public ActionDisplayList(Action notifyUpdate)
    {
        this.notifyUpdate = notifyUpdate;
    }

    public LocalizedString? this[string key]
    {
        get => _dictionary[key];
        set
        {
            if (value == null)
            {
                _dictionary.Remove(key);
                notifyUpdate();
                return;
            }

            _dictionary[key] = value.Value;
            notifyUpdate();
        }
    }

    public LocalizedString GetActive() => _dictionary.Last().Value;

    public bool HasActive() => _dictionary.Count != 0;

    public IEnumerator<KeyValuePair<string, LocalizedString>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
}
