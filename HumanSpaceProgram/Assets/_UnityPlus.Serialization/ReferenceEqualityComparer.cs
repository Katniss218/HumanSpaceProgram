using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
        public new bool Equals( object x, object y ) => ReferenceEquals( x, y );
        public int GetHashCode( object obj ) => RuntimeHelpers.GetHashCode( obj );
    }
}
