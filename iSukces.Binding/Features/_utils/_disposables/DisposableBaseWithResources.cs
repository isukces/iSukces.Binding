using System;

namespace iSukces.Binding
{
    public abstract class DisposableBaseWithResources : DisposableBase
    {
        private IDisposable _usedResources;

        protected DisposableBaseWithResources(IDisposable usedResources)
            : base(DisposingStateBehaviour.None)
        {
            _usedResources = usedResources;
        }

        protected override void DisposeInternal(bool disposing)
        {
            _usedResources?.Dispose();
            _usedResources = null;
            base.DisposeInternal(disposing);
        }
    }
}
