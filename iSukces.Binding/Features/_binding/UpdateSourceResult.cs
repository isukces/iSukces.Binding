using System;
using System.Reflection;

namespace iSukces.Binding
{
    public sealed class UpdateSourceResult
    {
        private UpdateSourceResult()
        {
        }

        private UpdateSourceResult(Exception exception, object value, UpdateSourceResultStatus status)
        {
            Exception = exception;
            Value     = value;
            Status    = status;
        }

        private UpdateSourceResult(UpdateSourceResultStatus status)
        {
            Status = status;
        }

        public static UpdateSourceResult FromException(Exception exception, object value)
        {
            if (exception.InnerException != null)
                if (exception is TargetInvocationException)
                {
                    return FromException(exception.InnerException, value);
                }

            return new UpdateSourceResult(exception, value, UpdateSourceResultStatus.Exception);
        }

        public static UpdateSourceResult OkFromValue(object value)
        {
            return new UpdateSourceResult(null, value, UpdateSourceResultStatus.Ok);
        }


        public override string ToString()
        {
            return Status switch
            {
                UpdateSourceResultStatus.Ok => nameof(UpdateSourceResultStatus.Ok),
                UpdateSourceResultStatus.OkUnableToReadBack => nameof(UpdateSourceResultStatus.OkUnableToReadBack),
                UpdateSourceResultStatus.Special => nameof(UpdateSourceResultStatus.Special),
                UpdateSourceResultStatus.NotSet => nameof(UpdateSourceResultStatus.NotSet),
                UpdateSourceResultStatus.InvalidValue => nameof(UpdateSourceResultStatus.InvalidValue),
                UpdateSourceResultStatus.Exception => "Exception " + Exception?.Message,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #region properties

        public Exception Exception { get; }
        public object    Value     { get; }

        public UpdateSourceResultStatus Status { get; }

        #endregion

        #region Fields

        public static readonly UpdateSourceResult Special = new(UpdateSourceResultStatus.Special);
        public static readonly UpdateSourceResult NotSet = new(UpdateSourceResultStatus.NotSet);
        public static readonly UpdateSourceResult InvalidValue = new(UpdateSourceResultStatus.InvalidValue);
        public static readonly UpdateSourceResult OkUnableToReadBack = new(UpdateSourceResultStatus.OkUnableToReadBack);

        #endregion
    }

    public enum UpdateSourceResultStatus
    {
        Ok,
        OkUnableToReadBack,
        Special,
        NotSet,
        InvalidValue,
        Exception
    }
}
