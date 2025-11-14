namespace PixiEditor.Models.Config;

public interface IMergeable<T>
{
    T TryMergeWith(T other);
}
