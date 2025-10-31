using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.OverridableValueProviders;

namespace HSP.Spatial
{
    public sealed class SpatialDataProviderModeAttribute : Attribute
    {
        public string Mode { get; set; }

        public SpatialDataProviderModeAttribute( string mode )
        {
            this.Mode = mode;
        }
    }

    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public sealed class SpatialDataProviderAttribute : Attribute
    {
        /// <summary>
        /// The type of the class that contains the registries.
        /// </summary>
        public Type Registry { get; set; }

        /// <summary>
        /// The ID of this provider.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Optional, the list of all providers that will be prevented from running when this provider is added.
        /// </summary>
        public string[] Blacklist { get; set; }

        /// <summary>
        /// Optional, this provider will run before the specified providers.
        /// </summary>
        public string[] Before { get; set; }

        /// <summary>
        /// Optional, this provider will run after the specified providers.
        /// </summary>
        public string[] After { get; set; }

        public SpatialDataProviderAttribute( Type dataId, string id )
        {
            this.Registry = dataId;
            this.ID = id;
        }

        private static OverridableValueProvider<T, TResult> CreateProvider<T, TResult>( string id, MethodInfo method, SpatialDataProviderAttribute classAttr, SpatialDataProviderModeAttribute modeAttr )
        {
            var del = (Func<T, TResult>)Delegate.CreateDelegate( typeof( Func<T, TResult> ), method );

            return new OverridableValueProvider<T, TResult>( id, del, classAttr.Blacklist, classAttr.Before, classAttr.After );
        }

        private static void ProcessMethodForProvider( Type type, MethodInfo providerMethod, Type providerInputType, Type providerResultType, SpatialDataProviderAttribute classAttr, SpatialDataProviderModeAttribute methodAttr )
        {
            var providerRegistryType = classAttr.Registry;
            FieldInfo registryField = providerRegistryType.GetField( $"_providers_{methodAttr.Mode}", BindingFlags.NonPublic | BindingFlags.Static );
            if( registryField == null )
            {
                // info: either the mode is mispelled, or the provider registry doesn't support that mode of provider.
                Debug.LogWarning( $"No registry field found for mode `{methodAttr.Mode}` (`_providers_{ methodAttr.Mode}`). Check mode spelling on method `{providerMethod.Name}` from `{type.FullName}` and ensure that `{providerRegistryType.FullName}` supports this mode." );
                return;
            }

            Type registryFieldType = registryField.FieldType;
            if( !registryFieldType.IsGenericType || registryFieldType.GetGenericTypeDefinition() != typeof( OverridableValueProviderRegistry<,> ) )
            {
                // info: possible incorrect type target on the provider.
                Debug.LogWarning( $"Field `{providerRegistryType.FullName}.{registryField.Name}` is not an `{typeof( OverridableValueProviderRegistry<,> ).Name}`." );
                return;
            }

            Type[] genArgs = registryFieldType.GetGenericArguments();
            if( genArgs.Length != 2 )
            {
                Debug.LogWarning( $"Provider registry field `{type.FullName}.{registryField.Name}` signature is not valid." );
                return;
            }
            if( genArgs[0] != providerInputType || genArgs[1] != providerResultType )
            {
                // info: provider signature doesn't match the correct signature expected by the provider registry.
                // Whoever created the provider needs to change it so that it matches.
                Debug.LogWarning( $"Method signature `static {providerResultType.Name} {providerMethod.Name}({providerInputType.Name} input)` does not match expected `static {genArgs[1].Name} {providerMethod.Name}({genArgs[0].Name} input)` for `{providerRegistryType.FullName}.{registryField.Name}`." );
                return;
            }

            object registry = registryField.GetValue( null );
            if( registry == null )
            {
                Debug.LogWarning( $"Registry field `{providerRegistryType.FullName}.{registryField.Name}` is null." );
                return;
            }

            MethodInfo addMethod = registryFieldType.GetMethod( "TryAddProvider" ); // bool TryAddProvider( provider ) // no generics on method.
            if( addMethod == null )
            {
                Debug.LogWarning( $"Registry field `{providerRegistryType.FullName}.{registryField.Name}` is null (uninitialized). The registry field should have an inline initializer." );
                return;
            }

            try
            {
                MethodInfo methodInfo = typeof( SpatialDataProviderAttribute ).GetMethod( "CreateProvider", BindingFlags.NonPublic | BindingFlags.Static );
                object provider = methodInfo.MakeGenericMethod( new Type[] { providerInputType, providerResultType } )
                    .Invoke( null, new object[] { classAttr.ID, providerMethod, classAttr, methodAttr } );

                bool isSuccess = (bool)addMethod.Invoke( registry, new object[] { provider } );
                if( !isSuccess )
                {
                    Debug.LogWarning( $"Duplicate provider ID `{classAttr.ID}` already exists for mode `{methodAttr.Mode}` in `{providerRegistryType.FullName}.{registryField.Name}`. Skipping registration." );
                    return;
                }
                Debug.Log( $"Successfully registered provider `{providerMethod.Name}` from `{type.FullName}` (ID: `{classAttr.ID}`) to `{providerRegistryType.FullName}` for mode `{methodAttr.Mode}`." );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to invoke TryAddProvider on registry `{registryField.Name}` in `{providerRegistryType.FullName}`: {ex.Message}." );
                Debug.LogException( ex );
            }
        }

        /// <summary>
        /// Searches for "value providers" in the specified assemblies and registers them in their corresponding registries. <br/>
        /// This method is idempotent.
        /// </summary>
        public static void RegisterValueProviders( IEnumerable<Assembly> assemblies )
        {
            foreach( var assembly in assemblies )
            {
                Type[] assemblyTypes = assembly.GetTypes();
                foreach( var type in assemblyTypes )
                {
                    if( !type.IsAbstract || !type.IsSealed )
                        continue; // only static classes.

                    try
                    {
                        IEnumerable<SpatialDataProviderAttribute> classAttrs = type.GetCustomAttributes<SpatialDataProviderAttribute>();
                        if( classAttrs == null || !classAttrs.Any() )
                        {
                            continue;
                        }

                        MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                        foreach( var providerMethod in methods )
                        {
                            try
                            {
                                IEnumerable<SpatialDataProviderModeAttribute> methodAttrs = providerMethod.GetCustomAttributes<SpatialDataProviderModeAttribute>();
                                if( methodAttrs == null || !methodAttrs.Any() )
                                {
                                    continue;
                                }

                                // Validate method signature: must be static (already filtered), return non-void, exactly one parameter
                                var parameters = providerMethod.GetParameters();
                                if( parameters.Length != 1 || providerMethod.ReturnType == typeof( void ) )
                                {
                                    Debug.LogWarning( $"Method `{type.FullName}.{providerMethod.Name}` has invalid signature for data provider registration. Expected: `static TResult Method( T input )`." );
                                    continue;
                                }

                                Type providerInputType = parameters[0].ParameterType;
                                Type providerResultType = providerMethod.ReturnType;

                                if( providerMethod.IsGenericMethodDefinition )
                                {
                                    Debug.LogWarning( $"Method `{providerMethod.Name}` in `{type.FullName}` (assembly: `{assembly.FullName}`) is generic and cannot be registered as a data provider. Remove generics from the method signature." );
                                    continue;
                                }

                                if( parameters[0].IsOut || parameters[0].ParameterType.IsByRef )
                                {
                                    Debug.LogWarning( $"Method `{providerMethod.Name}` in `{type.FullName}` (assembly: `{assembly.FullName}`) has a ref/out parameter, which is not supported for data providers. Use value types only." );
                                    continue;
                                }

                                foreach( var modeAttr in methodAttrs )
                                {
                                    // register to field '_providers_{mode}' on class 'classAttr.Registry' (if exists).

                                    foreach( var mainAttr in classAttrs )
                                    {
                                        ProcessMethodForProvider( type, providerMethod, providerInputType, providerResultType, mainAttr, modeAttr );
                                    }
                                }
                            }
                            catch( Exception ex )
                            {
                                Debug.LogError( $"Error processing provider method `{providerMethod.Name}` in class `{type.FullName}` from assembly `{assembly.FullName}`: {ex.Message}." );
                                Debug.LogException( ex );
                            }
                        }
                    }
                    catch( TypeLoadException ex )
                    {
                        Debug.LogError( $"Failed to load type `{ex.TypeName}` in assembly `{assembly.FullName}` (e.g., missing dependency): {ex.Message}. Ensure all referenced types/assemblies are available." );
                        Debug.LogException( ex );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Error loading/processing types in assembly `{assembly.FullName}`: {ex.Message}. Verify that the assembly is compatible and hasn't been corrupted." );
                        Debug.LogException( ex );
                    }
                }
            }
        }
    }
}