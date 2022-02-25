using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;
using ChangeableDocument.ChangeUpdateInfos;

namespace ChangeableDocument.Changes
{
    internal abstract class UpdateableChange<TargetT> : Change<TargetT>
    {
        public IChangeInfo? Update(IChangeable target, IUpdateInfo update)
        {
            if (!Initialized)
                throw new Exception("Can't update uninitialized change");
            if (Applied)
                throw new Exception("The change has already been applied");
            if (target is not TargetT conv)
                throw new Exception("Couldn't convert changeable");
            return DoUpdate(conv, update);
        }
        protected abstract IChangeInfo? DoUpdate(TargetT target, IUpdateInfo update);
    }
}
