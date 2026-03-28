using System;
using System.Linq;
using System.Reflection;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    /// <summary>
    /// Provides built-in descriptors for standard .NET types via the Provider system.
    /// This allows these types to be overridden by user providers if registered in a specific context.
    /// </summary>
    internal static class ReflectionProviders
    {
        [MapsInheritingFrom( typeof( MethodInfo ) )]
        private static IDescriptor ProvideMethodInfo() => new PrimitiveConfigurableDescriptor<MethodInfo>(
            ( v, w, c ) =>
            {
                SerializedArray parameters = new SerializedArray(
                    v.GetParameters().Select( p => p.ParameterType.SerializeType() )
                );

                w.Data = new SerializedObject()
                {
                    { "declaring_type", v.DeclaringType.SerializeType() },
                    { "identifier", v.Name },
                    { "parameters", parameters }
                };
            },
            ( d, c ) =>
            {
                Type declaringType = d["declaring_type"].DeserializeType();
                string name = (string)d["identifier"];

                Type[] parameters =
                    ((SerializedArray)d["parameters"])
                    .Select( p => p.DeserializeType() )
                    .ToArray();

                return declaringType.GetMethod(
                    name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    parameters,
                    null
                );
            }
        );

        [MapsInheritingFrom( typeof( FieldInfo ) )]
        private static IDescriptor ProvideFieldInfo() => new PrimitiveConfigurableDescriptor<FieldInfo>(
            ( v, w, c ) =>
            {
                w.Data = new SerializedObject()
                {
                    { "declaring_type", v.DeclaringType.SerializeType() },
                    { "identifier", v.Name }
                };
            },
            ( d, c ) =>
            {
                Type declaringType = d["declaring_type"].DeserializeType();
                string name = (string)d["identifier"];

                return declaringType.GetField(
                    name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                );
            }
        );

        [MapsInheritingFrom( typeof( PropertyInfo ) )]
        private static IDescriptor ProvidePropertyInfo() => new PrimitiveConfigurableDescriptor<PropertyInfo>(
            ( v, w, c ) =>
            {
                w.Data = new SerializedObject()
                {
                    { "declaring_type", v.DeclaringType.SerializeType() },
                    { "identifier", v.Name }
                };
            },
            ( d, c ) =>
            {
                Type declaringType = d["declaring_type"].DeserializeType();
                string name = (string)d["identifier"];

                return declaringType.GetProperty(
                    name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                );
            }
        );

        [MapsInheritingFrom( typeof( ConstructorInfo ) )]
        private static IDescriptor ProvideConstructorInfo() => new PrimitiveConfigurableDescriptor<ConstructorInfo>(
            ( v, w, c ) =>
            {
                SerializedArray parameters = new SerializedArray(
                    v.GetParameters().Select( p => p.ParameterType.SerializeType() )
                );

                w.Data = new SerializedObject()
                {
                    { "declaring_type", v.DeclaringType.SerializeType() },
                    { "parameters", parameters }
                };
            },
            ( d, c ) =>
            {
                Type declaringType = d["declaring_type"].DeserializeType();

                Type[] parameters =
                    ((SerializedArray)d["parameters"])
                    .Select( p => p.DeserializeType() )
                    .ToArray();

                return declaringType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    parameters,
                    null
                );
            }
        );

        [MapsInheritingFrom( typeof( Assembly ) )]
        private static IDescriptor ProvideAssembly() => new PrimitiveConfigurableDescriptor<Assembly>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.FullName,
            ( d, c ) => Assembly.Load( (string)d )
        );

        [MapsInheritingFrom( typeof( Module ) )]
        private static IDescriptor ProvideModule() => new PrimitiveConfigurableDescriptor<Module>(
            ( v, w, c ) =>
                w.Data = new SerializedObject()
                {
                    { "assembly", (SerializedPrimitive)v.Assembly.FullName },
                    { "identifier", (SerializedPrimitive)v.Name }
                },
            ( d, c ) =>
            {
                Assembly assembly = Assembly.Load( (string)d["assembly"] );
                return assembly.GetModule( (string)d["identifier"] );
            }
        );
    }
}