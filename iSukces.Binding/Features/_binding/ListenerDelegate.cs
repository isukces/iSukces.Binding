using JetBrains.Annotations;

namespace iSukces.Binding
{
    public delegate void ListenerDelegate([NotNull] IValueInfo info);

    public enum ListenerDelegateKind
    {
        UpdateSource,
        ValueChanged,
        StartBinding,
        EndBinding
    }
}
