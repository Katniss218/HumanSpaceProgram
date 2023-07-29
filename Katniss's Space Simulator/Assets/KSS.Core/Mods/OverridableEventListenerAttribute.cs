using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.OverridableEvents;

namespace KSS.Core.Mods
{
    /// <summary>
    /// Specifies that a method should be run when a specified overridable event is fired.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class OverridableEventListenerAttribute : Attribute
    {
        public string EventID { get; set; }
        public string ID { get; set; }
        public string[] Blacklist { get; set; }

        public OverridableEventListenerAttribute( string eventId, string id )
        {
            this.EventID = eventId;
            this.ID = id;
        }

        //
        // ---
        //

        private static bool AcceptMethodSignature( MethodInfo method )
        {
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 1
                && parameters[0].ParameterType == typeof( object )
                && method.ReturnType == typeof( void );
        }

        private static void ProcessMethod( OverridableEventListenerAttribute attr, MethodInfo method )
        {
            ParameterInfo[] parameters = method.GetParameters();

            if( !AcceptMethodSignature( method ) )
            {
                Debug.LogWarning( $"Ignoring a `{nameof( OverridableEventListenerAttribute )}` attribute applied to method `{method.Name}` which has an incorrect signature." );
                return;
            }

            Action<object> methodDelegate = (Action<object>)Delegate.CreateDelegate( typeof( Action<object> ), method );

            OverridableEvent.Instance.TryCreate( attr.EventID );
            OverridableEvent.Instance.TryAddListener( attr.EventID, new OverridableEventListener<Action<object>>() { id = attr.ID, blacklist = attr.Blacklist, func = methodDelegate } );
        }

        /// <summary>
        /// Searches for autorunning methods in the specified assemblies and registers them as listeners.
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
                            OverridableEventListenerAttribute attr = method.GetCustomAttribute<OverridableEventListenerAttribute>();
                            if( attr == null )
                            {
                                continue;
                            }

                            ProcessMethod( attr, method );
                        }
                        catch( TypeLoadException ex )
                        {
                            Debug.LogWarning( $"Couldn't resolve a type from the mod `{assembly.FullName}`: {ex.Message}." );
                        }
                    }
                }
            }
        }
    }
}