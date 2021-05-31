using System;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace iSukces.Binding
{
    /// <summary>
    ///     Helps to build a bunch of bindings based on the same bining source
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public sealed class BindingBuilder<TSource>
    {
        public BindingBuilder(TSource source, BindingManager bindingManager)
        {
            _source         = source;
            _bindingManager = bindingManager;
        }

        public BindingBuilder From(Expression<Func<TSource, object>> pathExpression)
        {
            var a = _bindingManager.From(_source, pathExpression);
            if (DefaultListenerDispatcher != null)
                a.ListenerDispatcher = DefaultListenerDispatcher;
            return a;
        }

        public Dispatcher DefaultListenerDispatcher { get; set; }
        
        private readonly BindingManager _bindingManager;
        private readonly TSource _source;
    }
}
