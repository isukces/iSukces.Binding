namespace iSukces.Binding
{
    public sealed class MinStringLengthBindingValidator : NotNullBindingValidator
    {
        public MinStringLengthBindingValidator(int minLength) { _minLength = minLength; }

        public override BindingValidatorResult Check(object value)
        {
            var result = base.Check(value);
            if (!result.IsOk)
                return result;
            var s = value?.ToString();
            if (string.IsNullOrEmpty(s))
                return ValueCantBeEmpty;

            if (s.Length >= _minLength) return BindingValidatorResult.Success;
            var noun = Nouns.Choose(_minLength, "znak", "znaki", "znaków");
            return $"Wartość nie może być krótsza niż {_minLength} {noun}.";
        }

        private readonly int _minLength;
    }
}
