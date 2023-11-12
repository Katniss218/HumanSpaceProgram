using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityPlus.Serialization;

namespace KSS.Core.Mods
{
    public static class IReferenceMap_Ex_NamespacedIdentifier
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData WriteNamespacedIdentifier( this IReverseReferenceMap _, NamespacedIdentifier nid )
        {
            return (SerializedPrimitive)nid.ToString();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static NamespacedIdentifier ReadNamespacedIdentifier( this IForwardReferenceMap _, SerializedData data )
        {
            return NamespacedIdentifier.Parse( (string)data );
        }
    }
}