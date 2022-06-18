using System.Diagnostics.CodeAnalysis;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Document : IChangeable, IReadOnlyDocument, IDisposable
{
    IReadOnlyFolder IReadOnlyDocument.StructureRoot => StructureRoot;
    IReadOnlySelection IReadOnlyDocument.Selection => Selection;
    IReadOnlyStructureMember? IReadOnlyDocument.FindMember(Guid guid) => FindMember(guid);
    IReadOnlyList<IReadOnlyStructureMember> IReadOnlyDocument.FindMemberPath(Guid guid) => FindMemberPath(guid);
    IReadOnlyStructureMember IReadOnlyDocument.FindMemberOrThrow(Guid guid) => FindMemberOrThrow(guid);
    (IReadOnlyStructureMember, IReadOnlyFolder) IReadOnlyDocument.FindChildAndParentOrThrow(Guid guid) => FindChildAndParentOrThrow(guid);

    public static VecI DefaultSize { get; } = new VecI(64, 64);
    internal Folder StructureRoot { get; } = new() { GuidValue = Guid.Empty };
    internal Selection Selection { get; } = new();
    public VecI Size { get; set; } = DefaultSize;
    public bool HorizontalSymmetryAxisEnabled { get; set; }
    public bool VerticalSymmetryAxisEnabled { get; set; }
    public int HorizontalSymmetryAxisY { get; set; }
    public int VerticalSymmetryAxisX { get; set; }

    public void Dispose()
    {
        StructureRoot.Dispose();
        Selection.Dispose();
    }

    public void ForEveryReadonlyMember(Action<IReadOnlyStructureMember> action) => ForEveryReadonlyMember(StructureRoot, action);
    public void ForEveryMember(Action<StructureMember> action) => ForEveryMember(StructureRoot, action);

    private void ForEveryReadonlyMember(IReadOnlyFolder folder, Action<IReadOnlyStructureMember> action)
    {
        foreach (var child in folder.Children)
        {
            action(child);
            if (child is IReadOnlyFolder innerFolder)
                ForEveryReadonlyMember(innerFolder, action);
        }
    }

    private void ForEveryMember(Folder folder, Action<StructureMember> action)
    {
        foreach (var child in folder.Children)
        {
            action(child);
            if (child is Folder innerFolder)
                ForEveryMember(innerFolder, action);
        }
    }

    /// <summary>
    /// Check's if a member with the <paramref name="guid"/> exists
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <returns>True if the member can be found, otherwise false</returns>
    public bool HasMember(Guid guid)
    {
        var list = FindMemberPath(guid);
        return list.Count > 0;
    }

    /// <summary>
    /// Check's if a member with the <paramref name="guid"/> exists and is of type <typeparamref name="T"/>
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <returns>True if the member can be found and is of type <typeparamref name="T"/>, otherwise false</returns>
    public bool HasMember<T>(Guid guid) where T : StructureMember
    {
        var list = FindMemberPath(guid);
        return list.Count > 0 && list[0] is T;
    }
    
    /// <summary>
    /// Find's the member with the <paramref name="guid"/> or throws a ArgumentException if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    public StructureMember FindMemberOrThrow(Guid guid) => FindMember(guid) ?? throw new ArgumentException($"Could not find member with guid '{guid}'");

    /// <summary>
    /// Find's the member of type <typeparamref name="T"/> with the <paramref name="guid"/> or throws an exception
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    /// <exception cref="InvalidCastException">Thrown if the member is not of type <typeparamref name="T"/></exception>
    public T FindMemberOrThrow<T>(Guid guid) where T : StructureMember => (T)FindMember(guid)!;

    /// <summary>
    /// Find's the member with the <paramref name="guid"/> or returns null if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    public StructureMember? FindMember(Guid guid)
    {
        var list = FindMemberPath(guid);
        return list.Count > 0 ? list[0] : null;
    }

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> and returns if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    public bool TryFindMember(Guid guid, [NotNullWhen(true)] out StructureMember? member)
    {
        var list = FindMemberPath(guid);
        if (list.Count == 0)
        {
            member = null;
            return false;
        }

        member = list[0];
        return true;
    }

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> and returns if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <typeparam name="T">The type of the <see cref="StructureMember"/></typeparam>
    /// <returns>True if the member could be found and is of type <typeparamref name="T"/>, otherwise false</returns>
    public bool TryFindMember<T>(Guid guid, [NotNullWhen(true)] out T? member) where T : StructureMember
    {
        if (!TryFindMember(guid, out var structureMember) || structureMember is not T cast)
        {
            member = null;
            return false;
        }

        member = cast;
        return false;
    }

    public (StructureMember, Folder) FindChildAndParentOrThrow(Guid childGuid)
    {
        var path = FindMemberPath(childGuid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find child and parent");
        return (path[0], (Folder)path[1]);
    }

    public (StructureMember?, Folder?) FindChildAndParent(Guid childGuid)
    {
        var path = FindMemberPath(childGuid);
        return path.Count switch
        {
            1 => (path[0], null),
            > 1 => (path[0], (Folder)path[1]),
            _ => (null, null),
        };
    }

    public List<StructureMember> FindMemberPath(Guid guid)
    {
        var list = new List<StructureMember>();
        if (FillMemberPath(StructureRoot, guid, list))
            list.Add(StructureRoot);
        return list;
    }

    private bool FillMemberPath(Folder folder, Guid guid, List<StructureMember> toFill)
    {
        if (folder.GuidValue == guid)
        {
            return true;
        }

        foreach (var member in folder.Children)
        {
            if (member is Layer childLayer && childLayer.GuidValue == guid)
            {
                toFill.Add(member);
                return true;
            }
            if (member is Folder childFolder)
            {
                if (FillMemberPath(childFolder, guid, toFill))
                {
                    toFill.Add(childFolder);
                    return true;
                }
            }
        }
        return false;
    }
}
