using System.Buffers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Helpers.Nodes;

public static class NodeAbbreviation
{
    private static readonly SearchValues<char> SearchFor = SearchValues.Create(['.']);

    
    public static bool IsAbbreviation(string value, out string? lastValue)
    {
        var span = value.AsSpan();

        int i = span.LastIndexOfAny(SearchFor);

        if (i == -1)
        {
            lastValue = null;
            return false;
        }

        lastValue = span[(i + 1)..].ToString();
        return true;
    }
    
    public static List<NodeTypeInfo>? FromString(string value, ICollection<NodeTypeInfo> allNodes)
    {
        var span = value.AsSpan();

        string lookFor = value;
        if (!span.ContainsAny(SearchFor))
        {
            return [allNodes.FirstOrDefault(SearchComparer)];
        }
        
        var list = new List<NodeTypeInfo>();

        var enumerator = new PartEnumerator(span, SearchFor);

        foreach (var name in enumerator)
        {
            lookFor = name.Name.ToString();
            var node = allNodes.First(SearchComparer);

            list.Add(node);
        }

        bool SearchComparer(NodeTypeInfo x) =>
            x.FinalPickerName.Value.Replace(" ", "").Contains(lookFor.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);

        return list;
    }

    private readonly ref struct PartResult(ReadOnlySpan<char> name, char? separator)
    {
        public ReadOnlySpan<char> Name { get; } = name;

        public char? Separator { get; } = separator;
    }

    private ref struct PartEnumerator(ReadOnlySpan<char> name, SearchValues<char> searchFor)
    {
        private bool isDone;
        private ReadOnlySpan<char> _remaining = name;
        private PartResult _current;

        public PartResult Current => _current;

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public PartEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (isDone)
                return false;

            int i = _remaining.IndexOfAny(searchFor);

            if (i == -1)
            {
                isDone = true;

                if (_remaining.Length == 0)
                {
                    return false;
                }

                _current = new PartResult(_remaining, null);
                return true;
            }

            _current = new PartResult(_remaining[..i], _remaining[i]);

            i++;
            _remaining = _remaining[i..];

            return true;
        }
    }
}
