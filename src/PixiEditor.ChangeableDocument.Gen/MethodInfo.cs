using System.Collections.Generic;

namespace PixiEditor.ChangeableDocument.Gen
{
    internal record struct MethodInfo(string Name, List<TypeWithName> Arguments, NamespacedType ContainingClass);
}
