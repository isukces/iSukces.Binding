using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public class BindingException : Exception
    {
        public BindingException([CanBeNull] string message, [CanBeNull] Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
