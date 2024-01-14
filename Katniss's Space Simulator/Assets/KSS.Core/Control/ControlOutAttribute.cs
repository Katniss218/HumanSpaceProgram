using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSS.Control
{
    /// <summary>
    /// Marks an event as an output for a control channel.
    /// </summary>
    [AttributeUsage( AttributeTargets.Event )]
    public sealed class ControlOutAttribute : Attribute
    {
        public ControlType ControlType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ControlOutAttribute( ControlType controlType, string name )
        {
            this.ControlType = controlType;
            this.Name = name;
        }

        public ControlOutAttribute( ControlType controlType, string name, string description )
        {
            this.ControlType = controlType;
            this.Name = name;
            this.Description = description;
        }

        public static IEnumerable<(EventInfo member, ControlOutAttribute attr)> GetControlOutputs( Component component )
        {
            // This is cacheable in a dict if needed.
            EventInfo[] events = component.GetType().GetEvents( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            return events.Select( e => (e, e.GetCustomAttribute<ControlOutAttribute>()) ).Where( e => e.Item2 != null );
        }
    }
}