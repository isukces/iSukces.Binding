using System;

namespace iSukces.Binding
{
    public static class BindingBootstrap
    {
        public static Func<object, bool> IsEnabledPredicate { get; set; }
    }
}
