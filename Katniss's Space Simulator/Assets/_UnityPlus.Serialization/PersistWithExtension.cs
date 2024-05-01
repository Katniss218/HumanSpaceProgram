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

        //private static readonly Dictionary<Type, Func<object, IReverseReferenceMap, SerializedObject>> _extensionGetObjects = new();
        //private static readonly Dictionary<Type, Action<object, SerializedObject, IForwardReferenceMap>> _extensionSetObjects = new();

        //private static readonly Dictionary<Type, Func<object, IReverseReferenceMap, SerializedData>> _extensionGetDatas = new();
        //private static readonly Dictionary<Type, Action<object, SerializedData, IForwardReferenceMap>> _extensionSetDatas = new();
        private static readonly Dictionary<Type, Delegate> _extensionGetObjects = new();
        private static readonly Dictionary<Type, Delegate> _extensionSetObjects = new();

        private static readonly Dictionary<Type, Delegate> _extensionGetDatas = new();
        private static readonly Dictionary<Type, Delegate> _extensionSetDatas = new();

        private static bool _isInitialized = false;

        public static void ReloadExtensionMethods()
        {
            _extensionGetObjects.Clear();
            _extensionSetObjects.Clear();

            _extensionGetDatas.Clear();
            _extensionSetDatas.Clear();

            List<Type> availableContainingClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( dt => dt.IsSealed && !dt.IsGenericType )
                .ToList();

            foreach( var cls in availableContainingClasses )
            {
                MethodInfo[] methods = cls.GetMethods( BindingFlags.Public | BindingFlags.Static );

                foreach( var method in methods )
                {
                    // Objects

                    if( method.Name == nameof( IPersistsObjects.GetObjects ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( SerializedObject )
                         && methodParams.Length == 2
                         && methodParams[1].ParameterType == typeof( IReverseReferenceMap ) )
                        {
                            Type methodType = typeof( Func<,,> ).MakeGenericType( methodParams[0].ParameterType, typeof( IReverseReferenceMap ), typeof( SerializedObject ) );
                            //var del = (Func<object, IReverseReferenceMap, SerializedObject>)Delegate.CreateDelegate( methodType, method );
                            var del = Delegate.CreateDelegate( methodType, method );

                            _extensionGetObjects.Add( methodParams[0].ParameterType, del );
                        }
                    }
                    else if( method.Name == nameof( IPersistsObjects.SetObjects ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( void )
                         && methodParams.Length == 3
                         && methodParams[1].ParameterType == typeof( SerializedObject )
                         && methodParams[2].ParameterType == typeof( IForwardReferenceMap ) )
                        {
                            if( methodParams[0].ParameterType.IsByRef )
                                continue;

                            Type methodType = typeof( Action<,,> ).MakeGenericType( methodParams[0].ParameterType, typeof( SerializedObject ), typeof( IForwardReferenceMap ) );
                            //var del = (Action<object, SerializedObject, IForwardReferenceMap>)Delegate.CreateDelegate( methodType, method );
                            var del = Delegate.CreateDelegate( methodType, method );

                            _extensionSetObjects.Add( methodParams[0].ParameterType, del );
                        }
                    }

                    // Data

                    else if( method.Name == nameof( IPersistsData.GetData ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( SerializedData )
                         && methodParams.Length == 2
                         && methodParams[1].ParameterType == typeof( IReverseReferenceMap ) )
                        {
                            Type methodType = typeof( Func<,,> ).MakeGenericType( methodParams[0].ParameterType, typeof( IReverseReferenceMap ), typeof( SerializedData ) );
                            //var del = (Func<object, IReverseReferenceMap, SerializedData>)Delegate.CreateDelegate( methodType, method );
                            var del = Delegate.CreateDelegate( methodType, method );

                            _extensionGetDatas.Add( methodParams[0].ParameterType, del );
                        }
                    }
                    else if( method.Name == nameof( IPersistsData.SetData ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( void )
                         && methodParams.Length == 3
                         && methodParams[1].ParameterType == typeof( SerializedData )
                         && methodParams[2].ParameterType == typeof( IForwardReferenceMap ) )
                        {
                            if( methodParams[0].ParameterType.IsByRef )
                                continue;

                            Type methodType = typeof( Action<,,> ).MakeGenericType( methodParams[0].ParameterType, typeof( SerializedData ), typeof( IForwardReferenceMap ) );
                            //var del = (Action<object, SerializedData, IForwardReferenceMap>)Delegate.CreateDelegate( methodType, method );
                            var del = Delegate.CreateDelegate( methodType, method );

                            _extensionSetDatas.Add( methodParams[0].ParameterType, del );
                        }
                    }
                }
            }

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