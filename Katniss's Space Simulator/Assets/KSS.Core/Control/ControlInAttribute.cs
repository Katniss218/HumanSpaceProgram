using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSS.Control
{
    /// <summary>
    /// Marks a method as an input for a control channel.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ControlInAttribute : Attribute
    {
        public ControlType ControlType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ControlInAttribute( ControlType controlType, string name )
        {
            this.ControlType = controlType;
            this.Name = name;
        }

        public ControlInAttribute( ControlType controlType, string name, string description )
        {
            this.ControlType = controlType;
            this.Name = name;
            this.Description = description;
        }

        public static IEnumerable<(MethodInfo member, ControlInAttribute attr)> GetControlInputs( Component component )
        {
            // This is cacheable in a dict if needed.
            MethodInfo[] methods = component.GetType().GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            return methods.Select( m => (m, m.GetCustomAttribute<ControlInAttribute>()) ).Where( m => m.Item2 != null );
        }
    }
}