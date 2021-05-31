using System.Reflection;

namespace iSukces.Binding
{
    public sealed class ReflectionPropertyInfo : IPropertyInfo
    {
        public ReflectionPropertyInfo(PropertyInfo propertyInfo, int priority,
            BindingFeatures features)
        {
            _propertyInfo = propertyInfo;
            Priority      = priority;
            Features      = features;
        }

        public IPropertyAccessor2 Create(object obj)
        {
            return new PropertyAccessorWithNpcSupport(obj, _propertyInfo);
        }

        public int             Priority { get; }
        public BindingFeatures Features { get; }
        private readonly PropertyInfo _propertyInfo;
    }
}
