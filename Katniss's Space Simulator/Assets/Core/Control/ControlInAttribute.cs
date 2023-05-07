using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Control
{
    /// <summary>
    /// Marks a method as an input for a control channel.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class ControlInAttribute : Attribute
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }

        public ControlInAttribute( string name, string displayName )
        {
            this.ID = name;
            this.DisplayName = displayName;
        }
    }
}