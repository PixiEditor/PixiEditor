using System.Collections;
using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;

public class Palette : IReadOnlyList<Color>, IEquatable<Palette>
{
    private readonly Color[] colors;

    public static Palette empty { get; } = new Palette(Array.Empty<Color>());
    public IReadOnlyList<Color> Colors => colors;
    public int Count => colors.Length;
    public Color this[int index] => colors[index];

    public Palette(IEnumerable<Color> colors)
    {
        this.colors = colors?.ToArray() ?? Array.Empty<Color>();
    }

    public Palette(Color[] colors)
    {
        this.colors = colors != null ? (Color[])(colors.Clone()) : Array.Empty<Color>();
    }

    public Palette CreateCopy() => new Palette(colors);

    public Color[] ToArray() => (Color[])(colors.Clone());

    public IEnumerator<Color> GetEnumerator() => ((IEnumerable<Color>)colors).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => colors.GetEnumerator();

    public bool Equals(Palette? other) => other is not null && colors.SequenceEqual(other.colors);

    public override bool Equals(object? obj) => Equals(obj as Palette);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (Color color in colors)
            hash.Add(color);
        return hash.ToHashCode();
    }
}
