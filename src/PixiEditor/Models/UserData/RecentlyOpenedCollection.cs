using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.Models.UserData;

internal class RecentlyOpenedCollection : System.Collections.ObjectModel.ObservableCollection<RecentlyOpenedDocument>
{
    public RecentlyOpenedDocument this[string path]
    {
        get
        {
            return Get(path);
        }
    }

    public RecentlyOpenedCollection()
    {
    }

    public RecentlyOpenedCollection(IEnumerable<RecentlyOpenedDocument> documents)
        : base(documents)
    {
    }

    public void Add(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        Add(Create(path));
    }

    public bool Contains(string path) => Get(path) is not null;

    public void Remove(string path) => Remove(Get(path));

    public int IndexOf(string path) => IndexOf(Get(path));

    public void Insert(int index, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        Insert(index, Create(path));
    }

    private static RecentlyOpenedDocument Create(string path) => new(path);

    private RecentlyOpenedDocument Get(string path) => this.FirstOrDefault(x => x.FilePath == path);
}
