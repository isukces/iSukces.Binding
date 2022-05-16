using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace iSukces.Binding
{
    public class BindingManager : DisposableBase, IDisposablesContainer
    {
        public BindingManager()
            : base(DisposingStateBehaviour.Throw)
        {
        }

        public void AddDisposable(IDisposable disposable) { _disposables.Add(disposable); }

        public IDisposable AddUpdateSourceAction(Action ac)
        {
            _updateSourceActions.Add(ac);
            return new DisposableAction(() =>
            {
                _updateSourceActions.Remove(ac);
            });
        }

        protected override void DisposeInternal(bool disposing)
        {
            var array = _disposables.ToArray();
            _disposables.Clear();
            for (var index = 0; index < array.Length; index++)
            {
                var disposable = array[index];
                disposable.Dispose();
            }

            base.DisposeInternal(disposing);
        }

        public BindingBuilder From<T>(T source, Expression<Func<T, object>> pathExpression)
        {
            var path = ExpressionTools.GetBindingPath(pathExpression);
            return From(source, path);
        }

        public BindingBuilder From<T>(T source, string path)
        {
            return From(source).WithPath(path).WithValidatorsFromAttributes();
        }


        public BindingBuilder From(object source)
        {
            if (!_sources.TryGetValue(source, out var wrapper))
                _sources[source] = wrapper = new BindingValueWrapper(source, this);
            return new BindingBuilder(wrapper);
        }

        public BindingBuilder<TSource> GetBuilder<TSource>(TSource source) { return new(source, this); }

        public void RemoveDisposable(IDisposable disposable) { _disposables.Remove(disposable); }

        public void UpdateAllSources()
        {
            for (var index = 0; index < _updateSourceActions.Count; index++)
            {
                var i = _updateSourceActions[index];
                i.Invoke();
            }
        }

        public IPropertyInfoProviderRegistry PropertyInfoProviderRegistry { get; set; }

        public string Name { get; set; }

        private readonly HashSet<IDisposable> _disposables = new(ReferenceEqualityComparer<IDisposable>.Instance);

        private readonly Dictionary<object, BindingValueWrapper> _sources = new(ReferenceEqualityComparer.Instance);

        private readonly List<Action> _updateSourceActions = new();
    }
}
