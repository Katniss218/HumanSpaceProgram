using System;
using UnityPlus.Serialization;

namespace KSS
{
    public static class Ghostable_object
    {
        public static SerializedData GetGhostData( this object obj, IReverseReferenceMap s )
        {
            throw new NotImplementedException($"Implement ghosting as a different kind of serialization mapping (?)");
           /* switch( obj )
            {
                case IGhostable o:
                    return o.GetGhostData( s );
                default:
                    return GhostableWithExtension.GetGhostData( obj, obj.GetType(), s );
            }*/
        }
    }
}