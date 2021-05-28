using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace iSukces.Binding
{
    public class BindingManager : DisposableBase
    {
        public BindingManager()
            : base(DisposingStateBehaviour.Throw)
        {
        }

        public void AddDisposable(IDisposable disposable) { _disposables.Add(disposable); }

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
            return From(source)
                .WithPath(path);
        }

        public BindingBuilder From(object source)
        {
            if (!_sources.TryGetValue(source, out var wrapper))
                _sources[source] = wrapper = new BindingValueWrapper(source, this);
            return new BindingBuilder(wrapper);
        }

        public void RemoveDisposable(IDisposable disposable) { _disposables.Remove(disposable); }

        private readonly HashSet<IDisposable> _disposables = new(ReferenceEqualityComparer<IDisposable>.Instance);

        private readonly Dictionary<object, BindingValueWrapper> _sources = new(ReferenceEqualityComparer.Instance);

        public B<TSource> GetBuilder<TSource>(TSource source)
        {
            return new(source, this);
        }

        public sealed class B<T>
        {
            private readonly T _source;
            private readonly BindingManager _bindingManager;

            public B(T source, BindingManager bindingManager)
            {
                _source              = source;
                _bindingManager = bindingManager;
            }
            
            public BindingBuilder From(Expression<Func<T, object>> pathExpression)
            {
                return _bindingManager.From(_source, pathExpression);
            }
        }
    }
}
