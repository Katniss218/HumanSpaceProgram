using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.OverridableEvents;

namespace KSS.Core
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

        public HSPEventListenerAttribute( string eventId, string id )
        {
            this.EventID = eventId;
            this.ID = id;
        }

        //
        // ---
        //

        private static bool IsValidMethodSignature( MethodInfo method )
        {
            // TODO - Potentially, this could check the type of the method against the type of the event in the future. To make it type-safe, without the `object` box.
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 1
                && parameters[0].ParameterType == typeof( object )
                && method.ReturnType == typeof( void );
        }

        private static void ProcessMethod( IEnumerable<HSPEventListenerAttribute> attrs, MethodInfo method )
        {
            ParameterInfo[] parameters = method.GetParameters();

            if( !IsValidMethodSignature( method ) )
            {
                Debug.LogWarning( $"Ignoring a `{nameof( HSPEventListenerAttribute )}` attribute applied to method `{method.Name}` which has an incorrect signature." );
                return;
            }

            Action<object> methodDelegate = (Action<object>)Delegate.CreateDelegate( typeof( Action<object> ), method );

            foreach( var attr in attrs )
            {
                HSPEvent.EventManager.TryCreate( attr.EventID );
                HSPEvent.EventManager.TryAddListener( attr.EventID, new OverridableEventListener<object>() { id = attr.ID, blacklist = attr.Blacklist, func = methodDelegate } );
            }
        }

        /// <summary>
        /// Searches for "autorunning" methods (i.e. with the attribute) in the specified assemblies and registers them as listeners.
        /// </summary>
        internal static void CreateEventsForAutorunningMethods( IEnumerable<Assembly> assemblies )
        {
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
                            Debug.LogWarning( $"The mod `{assembly.FullName}` expected a type `{ex.TypeName}` to exist, but it didn't.: {ex.Message}." );
                        }
                    }
                }
            }
        }
    }
}