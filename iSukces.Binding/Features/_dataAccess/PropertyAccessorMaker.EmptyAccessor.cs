namespace iSukces.Binding
{
    internal partial class PropertyAccessorMaker
    {
        private sealed class EmptyAccessor : IPropertyAccessor
        {
            private EmptyAccessor() { }

            public static bool AcceptsSource(object source)
            {
                if (source is null)
                    return true;
                if (source is BindingSpecial s)
                    return s.Kind is BindingSpecialKind.NotSet 
                        or BindingSpecialKind.Invalid 
                        or BindingSpecialKind.Unbound;
                return false;
            }

            public bool TryChangeSource(object source) { return AcceptsSource(source); }

            public object this[string propertyName] => BindingSpecial.NotSet;

            public UpdateSourceResult Write(string propertyName, object value)
            {
                throw new UnableToChangeReadOnlyPropertyException("");
            }

            public static EmptyAccessor Instance => NotSetPropertyAccessorHolder.SingleIstance;

            public static class NotSetPropertyAccessorHolder
            {
                public static readonly EmptyAccessor SingleIstance = new EmptyAccessor();
            }
        }
    }
}
