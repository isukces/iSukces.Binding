using System;

namespace iSukces.Binding
{
    public sealed class CombinedDisposable : DisposableBaseWithResources
    {
        private IDisposable _disposable;

        public CombinedDisposable(IDisposable usedResources, IDisposable disposable)
            : base(usedResources)
        {
            _disposable = disposable;
        }

        protected override void DisposeInternal(bool disposing)
        {
            _disposable?.Dispose();
            _disposable = null;
            base.DisposeInternal(disposing);
        }
    }
}
