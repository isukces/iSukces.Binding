using System;
using System.Reflection;

namespace iSukces.Binding
{
    public sealed class UpdateSourceResult
    {
        private UpdateSourceResult() { }

        private UpdateSourceResult(Exception exception, object value)
        {
            Exception = exception;
            Value     = value;
        }

        public static UpdateSourceResult FromException(Exception exception, object value)
        {
            if (exception.InnerException != null)
                if (exception is TargetInvocationException)
                {
                    return FromException(exception.InnerException, value);
                }
            return new(exception, value);
        }

        public override string ToString()
        {
            if (this == Ok) return nameof(Ok);
            if (this == Special) return nameof(Special);
            if (this == NotSet) return nameof(NotSet);
            if (this == InvalidValue) return nameof(InvalidValue);

            if (Exception != null)
                return "Exception " + Exception.Message;
            return base.ToString();
        }

        public static UpdateSourceResult Ok = new();
        public static UpdateSourceResult Special = new();
        public static UpdateSourceResult NotSet = new();
        public static UpdateSourceResult InvalidValue = new();
        public Exception Exception { get; }
        public object    Value     { get; }
    }
}
