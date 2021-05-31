using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public class UnableToChangeReadOnlyPropertyException : BindingException
    {
        public UnableToChangeReadOnlyPropertyException([CanBeNull] string message,
            [CanBeNull] Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
