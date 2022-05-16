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
        public BindingBuilder(TSource source, BindingManager manager)
        {
            _source         = source;
            Manager = manager;
        }

        public BindingBuilder From(Expression<Func<TSource, object>> pathExpression)
        {
            var a = Manager.From(_source, pathExpression);
            if (DefaultListenerDispatcher != null)
                a.ListenerDispatcher = DefaultListenerDispatcher;
            return a;
        }
        
        public BindingBuilder From(string path)
        {
            var a =  Manager.From(_source, path);
            if (DefaultListenerDispatcher != null)
                a.ListenerDispatcher = DefaultListenerDispatcher;
            return a;
        }

        public Dispatcher DefaultListenerDispatcher { get; set; }
        
        public BindingManager Manager { get; }
        private readonly TSource _source;

        
    }
}
