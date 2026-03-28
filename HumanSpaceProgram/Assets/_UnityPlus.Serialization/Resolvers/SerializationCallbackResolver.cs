using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization.Resolvers
{
    public static class SerializationCallbackResolver
    {
        public static (Action<object> onSerializing, Action<object> onSerialized, Action<object> onDeserializing, Action<object> onDeserialized) Resolve( Type type )
        {
            Action<object> onSerializing = null;
            Action<object> onSerialized = null;
            Action<object> onDeserializing = null;
            Action<object> onDeserialized = null;

            Type currentType = type;
            while( currentType != null && currentType != typeof( object ) )
            {
                var methods = currentType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly );

                foreach( var method in methods )
                {
                    var parameters = method.GetParameters();
                    if( parameters.Length != 1 || parameters[0].ParameterType != typeof( StreamingContext ) )
                        continue;

                    if( method.GetCustomAttribute<OnSerializingAttribute>() != null )
                    {
                        var m = method;
                        onSerializing = ((Action<object>)(( obj ) => m.Invoke( obj, new object[] { default( StreamingContext ) } ))) + onSerializing;
                    }

                    if( method.GetCustomAttribute<OnSerializedAttribute>() != null )
                    {
                        var m = method;
                        onSerialized = ((Action<object>)(( obj ) => m.Invoke( obj, new object[] { default( StreamingContext ) } ))) + onSerialized;
                    }

                    if( method.GetCustomAttribute<OnDeserializingAttribute>() != null )
                    {
                        var m = method;
                        onDeserializing = ((Action<object>)(( obj ) => m.Invoke( obj, new object[] { default( StreamingContext ) } ))) + onDeserializing;
                    }

                    if( method.GetCustomAttribute<OnDeserializedAttribute>() != null )
                    {
                        var m = method;
                        onDeserialized = ((Action<object>)(( obj ) => m.Invoke( obj, new object[] { default( StreamingContext ) } ))) + onDeserialized;
                    }
                }

                currentType = currentType.BaseType;
            }

            return (onSerializing, onSerialized, onDeserializing, onDeserialized);
        }
    }
}
