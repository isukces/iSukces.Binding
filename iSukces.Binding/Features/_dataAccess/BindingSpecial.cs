namespace iSukces.Binding
{
    public sealed class BindingSpecial
    {
        private BindingSpecial(BindingSpecialKind kind) { Kind = kind; }

        public override string ToString() { return Kind.ToString(); }

        /// <summary>
        ///     Source is null or doesn't contain requested property
        /// </summary>
        public static readonly BindingSpecial NotSet = new(BindingSpecialKind.NotSet);

        /// <summary>
        ///     Source has been unboud from parent
        /// </summary>
        public static readonly BindingSpecial Unbound = new(BindingSpecialKind.Unbound);


        /// <summary>
        /// Invalid value after conversion
        /// </summary>
        public static readonly BindingSpecial Invalid = new(BindingSpecialKind.Invalid);
        
        
        public        BindingSpecialKind Kind    { get; }
        
    }

    public enum BindingSpecialKind
    {
        /// <summary>
        ///     Source is null or doesn't contain requested property
        /// </summary>
        NotSet,

        /// <summary>
        ///     Source has been unboud from parent
        /// </summary>
        Unbound,
        
        /// <summary>
        /// Unable to convert to destination type
        /// </summary>
        Invalid
    }
}
