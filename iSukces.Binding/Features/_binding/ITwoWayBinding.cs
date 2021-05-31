using System;

namespace iSukces.Binding
{
    public interface ITwoWayBinding : IDisposable
    {
        UpdateSourceResult UpdateSource(object value);
    }

    internal class TwoWayBinding : DisposableBaseWithResources, ITwoWayBinding
    {
        public TwoWayBinding(IDisposable usedResources, Func<object, UpdateSourceResult> setValueAction)
            : base(usedResources)
        {
            _setValueAction = setValueAction;
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
    }
}
