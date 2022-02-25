using ChangeableDocument.Changeables;
using ChangeableDocument.ChangeInfos;

namespace ChangeableDocument.Changes
{
    internal abstract class Change<TargetT> : IChange
    {
        protected bool Initialized { get; private set; } = false;
        protected bool Applied { get; private set; } = false;
        public void Initialize(IChangeable target)
        {
            if (Initialized)
                throw new Exception("Already initialized");
            if (target is not TargetT conv)
                throw new Exception("Couldn't convert changeable");
            Initialized = true;
            DoInitialize(conv);
        }
        protected abstract void DoInitialize(TargetT target);

        public IChangeInfo? Apply(IChangeable target)
        {
            if (!Initialized)
                throw new Exception("Can't apply uninitialized change");
            if (Applied)
                throw new Exception("The change has already been applied");
            if (target is not TargetT conv)
                throw new Exception("Couldn't convert changeable");
            Applied = true;
            return DoApply(conv);
        }
        protected abstract IChangeInfo? DoApply(TargetT target);

        public IChangeInfo? Revert(IChangeable target)
        {
            if (!Initialized)
                throw new Exception("Can't revert uninitialized change");
            if (!Applied)
                throw new Exception("Can't revert a change that hasn't been applied");
            if (target is not TargetT conv)
                throw new Exception("Couldn't convert changeable");
            Applied = false;
            return DoRevert(conv);
        }
        protected abstract IChangeInfo? DoRevert(TargetT target);

    };
}
