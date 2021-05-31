using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace iSukces.Binding
{
    internal sealed class ReferenceEqualityComparer : object, IEqualityComparer<object>
    {
        private ReferenceEqualityComparer() { }


        public static Dictionary<T, T2> CreateDictionary<T, T2>()
            where T : class
        {
            return new Dictionary<T, T2>(Get<T>());
        }

        public static HashSet<T> CreateHashSet<T>()
            where T : class
        {
            return new HashSet<T>(Get<T>());
        }

        public static IEqualityComparer<T> Get<T>()
            where T : class
        {
            if (typeof(T) == typeof(object))
                return Instance;
            return ReferenceEqualityComparer<T>.Instance;
        }

        public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        public override string ToString() { return "nonGeneric " + nameof(ReferenceEqualityComparer); }

        public static ReferenceEqualityComparer Instance => InstanceHolder.SingleInstance;

        private static class InstanceHolder
        {
            public static readonly ReferenceEqualityComparer SingleInstance = new ReferenceEqualityComparer();
        }
    }

    internal class ReferenceEqualityComparer<T>
        : IEqualityComparer<T>
        where T : class
    {
        private ReferenceEqualityComparer() { }

        public bool Equals(T x, T y) { return ReferenceEquals(x, y); }

        public int GetHashCode(T obj) { return obj == null ? 0 : RuntimeHelpers.GetHashCode(obj); }

        public override string ToString()
        {
            return "generic " + nameof(ReferenceEqualityComparer) + "<" + typeof(T) + ">";
        }

        public static ReferenceEqualityComparer<T> Instance => InstanceHolder.SingleInstance;


        private static class InstanceHolder
        {
            public static readonly ReferenceEqualityComparer<T> SingleInstance = new ReferenceEqualityComparer<T>();
        }
    }
}
