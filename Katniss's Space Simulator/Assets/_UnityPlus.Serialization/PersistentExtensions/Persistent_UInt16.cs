using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class Persistent_UInt16
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this ushort value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ushort AsUInt16( this SerializedData data, IForwardReferenceMap l = null )
        {
            return (ushort)(SerializedPrimitive)data;
        }
    }
}