namespace iSukces.Binding
{
    internal partial class PropertyAccessorMaker
    {
        private sealed class Unbound : IPropertyAccessor
        {
            private Unbound() { }

            public static bool AcceptsSource(object source)
            {
                return source is BindingSpecial special && special.Kind == BindingSpecialKind.Unbound;
            }

            public bool TryChangeSource(object source) { return AcceptsSource(source); }
            
            public object this[string propertyName] => BindingSpecial.Unbound;
            public UpdateSourceResult Write(string propertyName, object value) { throw new UnableToChangeReadOnlyPropertyException("");  }

            public static Unbound Instance => UnboundHolder.SingleIstance;

            public static class UnboundHolder
            {
                public static readonly Unbound SingleIstance = new();
            }
        }
    }
}
