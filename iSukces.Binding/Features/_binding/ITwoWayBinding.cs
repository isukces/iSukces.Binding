using System;

namespace iSukces.Binding
{
    public interface ITwoWayBinding : IDisposable
    {
        UpdateSourceResult UpdateSource(object value);
    }

    internal sealed class TwoWayBinding : DisposableBase, ITwoWayBinding
    {
        public TwoWayBinding(Action disposeAction, Func<object, UpdateSourceResult> setValueAction)
            : base(DisposingStateBehaviour.None)
        {
            _disposeAction  = disposeAction;
            _setValueAction = setValueAction;
        }

        protected override void DisposeInternal(bool disposing)
        {
            _disposeAction?.Invoke();
            _disposeAction = null;
            base.DisposeInternal(disposing);
        }

        public UpdateSourceResult UpdateSource(object value)
        {
            if (_setValueAction is null)
                return UpdateSourceResult.NotSet;
            try
            {
                return _setValueAction.Invoke(value);
            }
            catch (Exception e)
            {
                return UpdateSourceResult.FromException(e, value);
            }
        }

        private readonly Func<object, UpdateSourceResult> _setValueAction;
        private Action _disposeAction;
    }
}
