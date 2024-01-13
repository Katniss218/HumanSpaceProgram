using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_object
    {
        public static SerializedData GetData( this object obj, IReverseReferenceMap s )
        {
            switch( obj )
            {
                case IPersistent o:
                    return o.GetData( s );
                case Component o:
                    return IPersistent_Component.GetData( o, s );
            }
            return null;
        }

        public static void SetData( this object obj, IForwardReferenceMap l, SerializedData data )
        {
            switch( obj )
            {
                case IPersistent o:
                    o.SetData( l, data ); break;
                case Component o:
                    IPersistent_Component.SetData( o, l, data ); break;
            }
        }
    }
}
