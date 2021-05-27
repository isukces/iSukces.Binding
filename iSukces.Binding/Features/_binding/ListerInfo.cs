using System;
using System.Runtime.CompilerServices;

namespace iSukces.Binding
{
    internal sealed class ListerInfo
    {
        public ListerInfo(ListenerDelegate action, Type expectedType, IBindingValueConverter converter)
        {
            Action       = action;
            ExpectedType = expectedType;
            Converter    = converter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object value, ListenerDelegateKind kind) { Action(value, kind); }

        public ListenerDelegate Action       { get; }
        public Type             ExpectedType { get; }

        public IBindingValueConverter Converter { get; }
    }
}
