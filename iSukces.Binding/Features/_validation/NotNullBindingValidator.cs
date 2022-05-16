namespace iSukces.Binding
{
    public class NotNullBindingValidator : BindingValidator
    {
        protected NotNullBindingValidator() { }

        public override BindingValidatorResult Check(object value)
        {
            return value is null ? ValueCantBeEmpty : BindingValidatorResult.Success;
        }

        public const string ValueCantBeEmpty = "Wartość nie może być pusta";

        public static NotNullBindingValidator Instance => NotNullBindingValidatorHolder.SingleIstance;

        private static class NotNullBindingValidatorHolder
        {
            public static readonly NotNullBindingValidator SingleIstance = new();
        }
    }
}
