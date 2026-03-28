using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization.Resolvers
{
    public static class SerializableMethodResolver
    {
        public static IMethodInfo[] GetMethods( Type type )
        {
            return Array.Empty<IMethodInfo>();

            //var methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
            //var methodList = new List<IMethodInfo>();

            //foreach( var method in methods )
            //{
            //    if( method.IsSpecialName )
            //        continue;
            //    if( method.DeclaringType == typeof( object ) || method.DeclaringType == typeof( ValueType ) || method.DeclaringType == typeof( Component ) || method.DeclaringType == typeof( MonoBehaviour ) || method.DeclaringType == typeof( ScriptableObject ) || method.DeclaringType == typeof( UnityEngine.Object ) )
            //        continue;

            //    bool hasContextMenu = method.GetCustomAttribute<ContextMenu>() != null;
            //    bool hasSerializeMethod = false;

            //    bool hasButtonAttribute = false;
            //    foreach( var attr in method.GetCustomAttributes() )
            //    {
            //        string attrName = attr.GetType().Name;
            //        if( attrName.Contains( "Button" ) || attrName.Contains( "ContextMenu" ) )
            //        {
            //            hasButtonAttribute = true;
            //            break;
            //        }
            //    }

            //    if( !hasContextMenu && !hasSerializeMethod && !hasButtonAttribute )
            //        continue;

            // #warning - add the try-catch like in other resolvers.
            //    methodList.Add( new ReflectionMethodInfo( method ) );
            //}

            //return methodList.ToArray();
        }
    }
}