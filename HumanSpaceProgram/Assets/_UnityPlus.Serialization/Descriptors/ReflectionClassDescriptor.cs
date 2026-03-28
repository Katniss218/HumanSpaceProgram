using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityPlus.Serialization.Resolvers;

namespace UnityPlus.Serialization.Descriptors
{
    public class ReflectionClassDescriptor<T> : CompositeDescriptor, ISerializationCallbackDescriptor
    {
        public override Type MappedType => typeof( T );
        private readonly IMemberInfo[] _members;
        private readonly IMethodInfo[] _methods;

        private readonly ConstructionStrategy _constructionStrategy;
        private readonly Func<object> _constructor;

        private readonly bool _implementsUnityCallback;
        private readonly Action<object> _onSerializing;
        private readonly Action<object> _onSerialized;
        private readonly Action<object> _onDeserializing;
        private readonly Action<object> _onDeserialized;

        public ReflectionClassDescriptor()
        {
            var type = typeof( T );

            var construction = ObjectConstructionResolver.Resolve( type );
            _constructionStrategy = construction.strategy;
            _constructor = construction.constructor;

            _members = SerializableMemberResolver.GetMembers( type );
            _methods = SerializableMethodResolver.GetMethods( type );

            var callbacks = SerializationCallbackResolver.Resolve( type );
            _onSerializing = callbacks.onSerializing;
            _onSerialized = callbacks.onSerialized;
            _onDeserializing = callbacks.onDeserializing;
            _onDeserialized = callbacks.onDeserialized;

            _implementsUnityCallback = typeof( ISerializationCallbackReceiver ).IsAssignableFrom( type );
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

            switch( _constructionStrategy )
            {
                case ConstructionStrategy.DefaultConstructor:
                case ConstructionStrategy.NonPublicConstructor:
                    return _constructor?.Invoke();
                case ConstructionStrategy.UninitializedObject:
                    return FormatterServices.GetUninitializedObject( typeof( T ) );
                case ConstructionStrategy.None:
                default:
                    return null;
            }
        }

        public void OnSerializing( object target, SerializationContext context )
        {
            if( _implementsUnityCallback )
                ((ISerializationCallbackReceiver)target).OnBeforeSerialize();
            _onSerializing?.Invoke( target );
        }

        public void OnSerialized( object target, SerializationContext context )
        {
            _onSerialized?.Invoke( target );
        }

        public void OnDeserializing( object target, SerializationContext context )
        {
            _onDeserializing?.Invoke( target );
        }

        public void OnDeserialized( object target, SerializationContext context )
        {
            if( _implementsUnityCallback )
                ((ISerializationCallbackReceiver)target).OnAfterDeserialize();
            _onDeserialized?.Invoke( target );
        }
    }
}