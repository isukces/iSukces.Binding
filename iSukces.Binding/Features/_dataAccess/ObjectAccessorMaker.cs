using System.Runtime.CompilerServices;

namespace iSukces.Binding
{
    internal static partial class ObjectAccessorMaker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SureAccessor(object source, ref IObjectAccessor accessor)
        {
            if (accessor is not null)
                return;
            if (EmptyAccessor.AcceptsSource(source))
                accessor = EmptyAccessor.Instance;
            else if (Unbound.AcceptsSource(source))
                accessor = Unbound.Instance;
            else
                accessor = new ReflectionAccessor(source);
        }
    }
}
