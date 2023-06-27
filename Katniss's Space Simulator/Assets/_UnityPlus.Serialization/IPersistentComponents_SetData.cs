using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;
using UnityEngine.Rendering;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistentComponents_SetData
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Component c, ILoader l, SerializedData json )
        {
            // component "data" means that the component (which is a referencable object) has already been added by an object action, and we're now reading its data.

            SerializedObject jsonObj = (SerializedObject)json;
            switch( c )
            {
                case IPersistent comp:
                    comp.SetData( l, jsonObj ); break;
                case Transform comp:
                    comp.SetData( l, jsonObj ); break;
                case MeshFilter comp:
                    comp.SetData( l, jsonObj ); break;
                case MeshRenderer comp:
                    comp.SetData( l, jsonObj ); break;
            }
        }
    }
}