using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Control
{
    /// <summary>
    /// Marks a field to be managed by the control setup system. <br/>
    /// The control will have a specific name next to it, and will show a specified description when hovered over.
    /// </summary>
    public sealed class NamedControlAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; } = null;

        public NamedControlAttribute( string name )
        {
            this.Name = name;
        }

        public NamedControlAttribute( string name, string description )
        {
            this.Name = name;
            this.Description = description;
        }
    }
    
    public abstract class ControlInputOutput
    {
        /// <summary>
        /// The transform that this "plug/socket" belongs (i.e. is attached) to.
        /// </summary>
        internal Transform transform { get; set; }
    }

    /// <summary>
    /// Inherit from this class to create your own control group with your own inputs and outputs.
    /// </summary>
    public abstract class ControlGroup : ControlInputOutput
    {

    }

    // "input" = on the left side.
    // "output" = on the right side.

    // parameters are reversed in their logic.

    /// <summary>
    /// Control parameter requirer
    /// </summary>
    public sealed class ControlParameterInput<T> : ControlInputOutput
    {
        internal Func<T> evt;

        public ControlParameterInput()
        {
        }

        /// <summary>
        /// Invokes the connected parameter getter to retrieve the parameter.
        /// </summary>
        /// <param name="value">If the returned value is `true`, contains the value of the connected parameter getter.</param>
        /// <returns>True if the value was returned successfully (i.e. connection is connected and healthy), otherwise false.</returns>
        public bool TryGet( out T value )
        {
            if( evt == null )
            {
                value = default;
                return false;
            }

            value = evt.Invoke();
            return true;
        }
    }

    /// <summary>
    /// Control parameter getter
    /// </summary>
    public sealed class ControlParameterOutput<T> : ControlInputOutput
    {
        internal Func<T> getter;

        /// <param name="getter">Returns the value of the parameter.</param>
        public ControlParameterOutput( Func<T> getter )
        {
            this.getter = getter;
        }
    }
    
    /// <summary>
    /// Controllee
    /// </summary>
    public sealed class ControllerInput<T> : ControlInputOutput
    {
        internal Action<T> onInvoke;

        /// <param name="signalResponse">The action to perform when a control signal is sent to this input.</param>
        public ControllerInput( Action<T> signalResponse )
        {
            this.onInvoke = signalResponse;
        }
    }

    /// <summary>
    /// Controller
    /// </summary>
    public sealed class ControllerOutput<T> : ControlInputOutput
    {
        internal event Action<T> Event;

        public void TrySendSignal( T signalValue )
        {
            Event?.Invoke( signalValue );
        }
    }

    public static class ControlUtils
    {
        public static void CacheControlInOutTransforms( Transform root )
        {

        }

        // arrays are supported on groups, and inputs/outputs. This will display as multiple of the entry right under each other. Possibly with a number for easier matching.

        /*public static IEnumerable<(MethodInfo member, ControllerInAttribute attr)> GetControlInputs( Component component )
        {
            // This is cacheable in a dict if needed.
            MethodInfo[] methods = component.GetType().GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            return methods.Select( m => (m, m.GetCustomAttribute<ControllerInAttribute>()) ).Where( m => m.Item2 != null );
        }

        public static IEnumerable<(EventInfo member, ControllerOutAttribute attr)> GetControlOutputs( Component component )
        {
            // This is cacheable in a dict if needed.
            EventInfo[] events = component.GetType().GetEvents( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            return events.Select( e => (e, e.GetCustomAttribute<ControllerOutAttribute>()) ).Where( e => e.Item2 != null );
        }*/
    }
}