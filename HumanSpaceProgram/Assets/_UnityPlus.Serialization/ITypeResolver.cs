using System;

namespace UnityPlus.Serialization
{
    public interface ITypeResolver
    {
        Type ResolveType( string typeName );
    }
}