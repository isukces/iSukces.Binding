namespace iSukces.Binding
{
    public delegate void ListenerDelegate(object value, ListenerDelegateKind kind);

    public enum ListenerDelegateKind
    {
        UpdateSource,
        ValueChanged,
        StartBinding,
        EndBinding
    }
}
