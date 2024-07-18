using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.OverridableEvents;

namespace HSP
{
    /// <summary>
    /// Specifies that a method should be run when a specified game event is fired.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to any static method with the signature `static void Method( object e )`.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    public class HSPEventListenerAttribute : Attribute
    {
        public string EventID { get; set; }
        public string ID { get; set; }
        public string[] Blacklist { get; set; }

        public string[] Before { get; set; }
        public string[] After { get; set; }

        public HSPEventListenerAttribute( string eventId, string id )
        {
            this.EventID = eventId;
            this.ID = id;
        }

        //
        // ---
        //

        enum ListenerType
        {
            Invalid = 0,
            ParameterDirect,
            ParameterDowncasted,
            Parameterless
        }

        /// <summary>
        /// Checks if the signature is valid, and the type of listener to generate for the given signature.
        /// </summary>
        private static ListenerType CheckValidSignatures( MethodInfo method )
        {
            if( !method.IsStatic )
                return ListenerType.Invalid;

            ParameterInfo[] parameters = method.GetParameters();

            if( parameters.Length > 1 )
                return ListenerType.Invalid;

            if( parameters.Length == 0 )
                return ListenerType.Parameterless;

            if( parameters[0].ParameterType == typeof( object ) && method.ReturnType == typeof( void ) )
                return ListenerType.ParameterDirect;

            if( method.ReturnType == typeof( void ) )
                return ListenerType.ParameterDowncasted;

            return ListenerType.Invalid;
        }

        private static Action<object> GenerateLambdaCallingParameterless( MethodInfo targetMethod )
        {
            // Generates a lambda that looks like this: `(object o) => targetMethod();`
            // 150x faster than InvokeDynamic, and doesn't require ugly modifications to the event system itself, not serializable though but we don't need that.

            ParameterExpression parameter = Expression.Parameter( typeof( object ), "o" );
            MethodCallExpression methodCallExpression = Expression.Call( null, targetMethod );

            return (Action<object>)Expression.Lambda( methodCallExpression, parameter ).Compile();
        }
        
        private static Action<object> GenerateLambdaCallingDowncasted( MethodInfo targetMethod, Type targetType )
        {
            // Generates a lambda that looks like this: `(object o) => targetMethod( (targetType)o );`
            // 150x faster than InvokeDynamic, and doesn't require ugly modifications to the event system itself, not serializable though but we don't need that.

            ParameterExpression parameter = Expression.Parameter( typeof( object ), "o" );
            UnaryExpression convertExpression = Expression.Convert( parameter, targetType );
            MethodCallExpression methodCallExpression = Expression.Call( null, targetMethod, convertExpression );

            return (Action<object>)Expression.Lambda( methodCallExpression, parameter ).Compile();
        }

        private static void ProcessMethod( IEnumerable<HSPEventListenerAttribute> attrs, MethodInfo method )
        {
            ParameterInfo[] parameters = method.GetParameters();

            ListenerType listenerType = CheckValidSignatures( method );
            if( listenerType == ListenerType.Invalid )
            {
                Debug.LogWarning( $"Ignoring a `{nameof( HSPEventListenerAttribute )}` attribute applied to method `{method.Name}` which has an incorrect signature." );
                return;
            }

            Action<object> methodDelegate;

            if( listenerType == ListenerType.ParameterDirect )
            {
                // if signature matches exactly, just call the method itself.
                methodDelegate = (Action<object>)Delegate.CreateDelegate( typeof( Action<object> ), method );
            }
            else if( listenerType == ListenerType.ParameterDowncasted )
            {
                methodDelegate = GenerateLambdaCallingDowncasted( method, parameters[0].ParameterType );
            }
            else if( listenerType == ListenerType.Parameterless )
            {
                methodDelegate = GenerateLambdaCallingParameterless( method );
            }
            else // unknown/unsupported type
            {
                return;
            }

            foreach( var attr in attrs )
            {
                HSPEvent.EventManager.TryCreate( attr.EventID );
                HSPEvent.EventManager.TryAddListener( attr.EventID, new OverridableEventListener<object>( attr.ID, attr.Blacklist, attr.Before, attr.After, methodDelegate ) );
            }
        }

        /// <summary>
        /// Searches for "autorunning" methods (i.e. with the attribute) in the specified assemblies and registers them as listeners.
        /// </summary>
        public static void CreateEventsForAutorunningMethods( IEnumerable<Assembly> assemblies )
        {
#warning TODO - this is public because I made it public temporarily . FIX LATER
            foreach( var assembly in assemblies )
            {
                Type[] assemblyTypes = assembly.GetTypes();
                foreach( var type in assemblyTypes )
                {
                    MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                    foreach( var method in methods )
                    {
                        try
                        {
                            IEnumerable<HSPEventListenerAttribute> attrs = method.GetCustomAttributes<HSPEventListenerAttribute>();
                            if( attrs == null || !attrs.Any() )
                            {
                                continue;
                            }

                            ProcessMethod( attrs, method );
                        }
                        catch( TypeLoadException ex )
                        {
                            Debug.LogError( $"The mod `{assembly.FullName}` expected a type `{ex.TypeName}` to exist, but it didn't.: {ex.Message}." );
                            Debug.LogException( ex );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"The mod `{assembly.FullName}` couldn't be loaded.: {ex.Message}." );
                            Debug.LogException( ex );
                        }
                    }
                }
            }
        }
    }
}