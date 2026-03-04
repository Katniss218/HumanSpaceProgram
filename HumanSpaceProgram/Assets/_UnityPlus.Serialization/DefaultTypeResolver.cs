
using System;

namespace UnityPlus.Serialization
{
    public class DefaultTypeResolver : ITypeResolver
    {
        public Type ResolveType( string typeName )
        {
            return Persistent_Type.ResolveType( typeName );
        }
    }
}