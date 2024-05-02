using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class PersistWithExtension
    {
        /*
        
        'extension' serialization mode.

        Extension mirrors the direct (manual) mode, but uses extension methods instead of implementing an interface.

        */

        private static readonly ExtensionMap _extensionGetObjects = new ExtensionMap( nameof( IPersistsObjects.GetObjects ), typeof( SerializedObject ), typeof( IReverseReferenceMap ) );
        private static readonly ExtensionMap _extensionSetObjects = new ExtensionMap( nameof( IPersistsObjects.SetObjects ), typeof( void ), typeof( SerializedObject ), typeof( IForwardReferenceMap ) );
        private static readonly ExtensionMap _extensionGetDatas = new ExtensionMap( nameof( IPersistsData.GetData ), typeof( SerializedData ), typeof( IReverseReferenceMap ) );
        private static readonly ExtensionMap _extensionSetDatas = new ExtensionMap( nameof( IPersistsData.SetData ), typeof( void ), typeof( SerializedData ), typeof( IForwardReferenceMap ) );

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetStaticTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( dt => dt.IsSealed && !dt.IsGenericType );
        }

        public static void ReloadExtensionMethods()
        {
            _extensionGetObjects.Reload();
            _extensionSetObjects.Reload();
            _extensionGetDatas.Reload();
            _extensionSetDatas.Reload();

            _isInitialized = true;
        }

        public static SerializedObject GetObjects( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_isInitialized )
                ReloadExtensionMethods();

            if( _extensionGetObjects.TryGetValue( objType, out var extensionMethod ) )
            {
                return (SerializedObject)extensionMethod.DynamicInvoke( obj, s );
            }
            return null;
        }

        public static void SetObjects( object obj, Type objType, SerializedObject data, IForwardReferenceMap l )
        {
            if( !_isInitialized )
                ReloadExtensionMethods();

            if( _extensionSetObjects.TryGetValue( objType, out var extensionMethod ) )
            {
                extensionMethod.DynamicInvoke( obj, data, l );
            }
        }

        public static SerializedData GetData( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_isInitialized )
                ReloadExtensionMethods();

            if( _extensionGetDatas.TryGetValue( objType, out var extensionMethod ) )
            {
                return (SerializedData)extensionMethod.DynamicInvoke( obj, s );
            }
            return null;
        }

        public static void SetData( object obj, Type objType, IForwardReferenceMap l, SerializedData data )
        {
            if( !_isInitialized )
                ReloadExtensionMethods();

            if( objType.IsValueType )
            {
                // pass by ref, if possible.
            }
            else
            {
                if( _extensionSetDatas.TryGetValue( objType, out var extensionMethod ) )
                {
                    extensionMethod.DynamicInvoke( obj, data, l );
                }
            }
        }
    }
}