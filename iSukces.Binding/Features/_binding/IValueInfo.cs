namespace iSukces.Binding
{
    public interface IValueInfo
    {
        ListenerDelegateKind Kind { get; }

        object Value { get; }

        object LastValidValue { get; }
    }


    internal class PrvValueInfo : IValueInfo
    {
        public PrvValueInfo(ListenerDelegateKind kind, object value, object lastValidValue)
        {
            Kind           = kind;
            Value          = value;
            LastValidValue = lastValidValue;
        }

        public static IValueInfo Make(IValueInfo info, object value)
        {
            return new PrvValueInfo(info.Kind, value, info.LastValidValue);
        }

        public ListenerDelegateKind Kind           { get; }
        public object               Value          { get; }
        public object               LastValidValue { get; }
    }
}
