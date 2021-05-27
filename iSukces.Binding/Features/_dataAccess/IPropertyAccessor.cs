namespace iSukces.Binding
{
    internal interface IPropertyAccessor
    {
        bool TryChangeSource(object source);
        object this[string propertyName] { get; }
        UpdateSourceResult Write(string propertyName, object value);
    }
}
