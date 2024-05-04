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
        public static SerializedData GetData( this NamespacedIdentifier identifier, IReverseReferenceMap l = null )
        {
            return (SerializedPrimitive)identifier.ToString();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static NamespacedIdentifier AsNamespacedIdentifier( this SerializedData data, IForwardReferenceMap l = null )
        {
            return NamespacedIdentifier.Parse( data.AsString() );
        }
    }
}