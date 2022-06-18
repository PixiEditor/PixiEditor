using System.Diagnostics.CodeAnalysis;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyDocument
{
    IReadOnlyFolder StructureRoot { get; }
    /// <summary>
    /// The selection of the document
    /// </summary>
    IReadOnlySelection Selection { get; }
    VecI Size { get; }
    /// <summary>
    /// Is the horizontal symmetry axis enabled (Mirrors top and bottom)
    /// </summary>
    bool HorizontalSymmetryAxisEnabled { get; }
    /// <summary>
    /// Is the vertical symmetry axis enabled (Mirrors left and right)
    /// </summary>
    bool VerticalSymmetryAxisEnabled { get; }
    /// <summary>
    /// The position of the horizontal symmetry axis (Mirrors top and bottom)
    /// </summary>
    int HorizontalSymmetryAxisY { get; }
    /// <summary>
    /// The position of the vertical symmetry axis (Mirrors top and bottom)
    /// </summary>
    int VerticalSymmetryAxisX { get; }
    /// <summary>
    /// Performs the specified action on each readonly member of the document
    /// </summary>
    void ForEveryReadonlyMember(Action<IReadOnlyStructureMember> action);
    /// <summary>
    /// Find's the member with the <paramref name="guid"/> or returns null if not found
    /// </summary>
    /// <param name="guid">The <see cref="IReadOnlyStructureMember.GuidValue"/> of the member</param>
    IReadOnlyStructureMember? FindMember(Guid guid);
    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> of type <typeparamref name="T"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    bool TryFindMember<T>(Guid guid, [NotNullWhen(true)] out T? member) where T : IReadOnlyStructureMember;
    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    bool TryFindMember(Guid guid, [NotNullWhen(true)] out IReadOnlyStructureMember? member);
    /// <summary>
    /// Find's the member with the <paramref name="guid"/> or throws a ArgumentException if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    IReadOnlyStructureMember FindMemberOrThrow(Guid guid);
    (IReadOnlyStructureMember, IReadOnlyFolder) FindChildAndParentOrThrow(Guid guid);
    IReadOnlyList<IReadOnlyStructureMember> FindMemberPath(Guid guid);
}
