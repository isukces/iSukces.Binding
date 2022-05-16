namespace iSukces.Binding
{
    public abstract class BindingValidator
    {
        public abstract BindingValidatorResult Check(object value);

        public static IGrammarNoun Nouns { get; set; } = new PolishGrammarNoun();
    }

    public sealed class BindingValidatorResult
    {
        public BindingValidatorResult(string errorMessage) { ErrorMessage = errorMessage?.Trim(); }

        public static implicit operator BindingValidatorResult(string errorMessage)
        {
            return new BindingValidatorResult(errorMessage);
        }

        public static readonly BindingValidatorResult Success = new(null);

        public string ErrorMessage { get; }
        public bool   IsOk         => string.IsNullOrEmpty(ErrorMessage);
    }
}
