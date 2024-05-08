using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public static class Persistent_List_T_
    {
        public static SerializedData AsSerialized<T>( this List<T> list, IReverseReferenceMap s )
        {

        }

        public static List<T> AsList<T>( this SerializedData data, IForwardReferenceMap l )
        {

        }

        public static SerializedData AsSerialized<T>( this T[] array, IReverseReferenceMap s )
        {

        }

        public static T[] AsArray<T>( this SerializedData data, IForwardReferenceMap l )
        {

        }
    }
}