using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS
{
    public static class GhostableWithExtension
    {
#warning TODO - replace with serialization contexts.
        private static readonly ExtensionMap _extensionGetGhostDatas = new ExtensionMap( nameof( IGhostable.GetGhostData ), typeof( SerializedData ), typeof( IReverseReferenceMap ) );

        private static bool _isInitialized = false;

        public static void ReloadExtensionMethods()
        {
            _extensionGetGhostDatas.Reload();

            _isInitialized = true;
        }

        public static SerializedData GetGhostData( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_isInitialized )
                ReloadExtensionMethods();

            if( _extensionGetGhostDatas.TryGetValue( objType, out var extensionMethod ) )
            {
                return (SerializedData)extensionMethod.DynamicInvoke( obj, s );
            }
            return null;
        }
    }
}