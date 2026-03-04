using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class ReflectionClassDescriptor<T> : CompositeDescriptor
    {
        public override Type MappedType => typeof( T );
        private readonly IMemberInfo[] _members;
        private readonly IMethodInfo[] _methods;
        private readonly Func<T> _constructor;

        private readonly bool _implementsUnityCallback;
        private readonly Action<object> _onSerializing;
        private readonly Action<object> _onDeserialized;

        public ReflectionClassDescriptor()
        {
            if( !typeof( T ).IsInterface && !typeof( ScriptableObject ).IsAssignableFrom( typeof( T ) ) && !typeof( Component ).IsAssignableFrom( typeof( T ) ) )
            {
                try
                {
                    var ctor = typeof( T ).GetConstructor( Type.EmptyTypes );
                    if( ctor != null || typeof( T ).IsValueType )
                    {
                        _constructor = AccessorUtils.CreateConstructor<T>();
                    }
                }
                catch { }
            }

            var fields = typeof( T ).GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            var memberList = new List<IMemberInfo>();

            foreach( var field in fields )
            {
                if( field.IsStatic )
                    continue;
                bool isPublic = field.IsPublic;
                bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
                bool hasNonSerialized = field.GetCustomAttribute<NonSerializedAttribute>() != null;

                if( hasNonSerialized )
                    continue;
                if( !isPublic && !hasSerializeField )
                    continue;

                memberList.Add( new ReflectionFieldInfo( field ) );
            }
            _members = memberList.ToArray();

            var methods = typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
            var methodList = new List<IMethodInfo>();

            foreach( var method in methods )
            {
                if( method.GetCustomAttribute<OnSerializingAttribute>() != null && method.GetParameters().Length == 1 )
                    _onSerializing = ( obj ) => method.Invoke( obj, new object[] { default( StreamingContext ) } );

                if( method.GetCustomAttribute<OnDeserializedAttribute>() != null && method.GetParameters().Length == 1 )
                    _onDeserialized = ( obj ) => method.Invoke( obj, new object[] { default( StreamingContext ) } );

                if( method.IsSpecialName )
                    continue;
                if( method.DeclaringType == typeof( object ) || method.DeclaringType == typeof( Component ) || method.DeclaringType == typeof( MonoBehaviour ) )
                    continue;

                methodList.Add( new ReflectionMethodInfo( method ) );
            }
            _methods = methodList.ToArray();

            _implementsUnityCallback = typeof( ISerializationCallbackReceiver ).IsAssignableFrom( typeof( T ) );
        }

        public override int GetStepCount( object target ) => _members.Length;
        public override IMemberInfo GetMemberInfo( int stepIndex ) => _members[stepIndex];

        public override int GetMethodCount() => _methods.Length;
        public override IMethodInfo GetMethodInfo( int methodIndex ) => _methods[methodIndex];

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            if( typeof( T ).IsInterface )
                return null;

            if( typeof( ScriptableObject ).IsAssignableFrom( typeof( T ) ) )
                return ScriptableObject.CreateInstance( typeof( T ) );

            if( _constructor != null )
                return _constructor();

            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                return null;
            }
        }

        public override void OnSerializing( object target, SerializationContext context )
        {
            if( _implementsUnityCallback )
                ((ISerializationCallbackReceiver)target).OnBeforeSerialize();
            _onSerializing?.Invoke( target );
        }

        public override void OnDeserialized( object target, SerializationContext context )
        {
            if( _implementsUnityCallback )
                ((ISerializationCallbackReceiver)target).OnAfterDeserialize();
            _onDeserialized?.Invoke( target );
        }
    }
}