using System;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace iSukces.Binding
{
    public static class BindingBuilderExtensions
    {
        public static void UpdateTargetWithDispatcher<T, TTargetProperty>(this BindingBuilder self,
            T targetObject,
            Expression<Func<T, TTargetProperty>> propertyExpression, Dispatcher dispatcher=null)
        {
            self.WithListenerDispatcher(dispatcher);
            var p    = ExpressionTools.GetBindingPath(propertyExpression);
            var prop = targetObject.GetType().GetProperty(p);
            if (prop is null)
                throw new Exception("Unable to find property " + p);
            self.CreateListener(info =>
            {
                var value = info.Value is BindingSpecial ? null : info.Value?.ToString();
                prop.SetValue(targetObject, value);
            }, typeof(TTargetProperty));
        }
    }
}
