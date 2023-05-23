using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Control
{
    /// <summary>
    /// Marks an event as an output for a control channel.
    /// </summary>
    [AttributeUsage( AttributeTargets.Event )]
    public class ControlOutAttribute : Attribute
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }

        public ControlOutAttribute( string name, string displayName )
        {
            this.ID = name;
            this.DisplayName = displayName;
        }
    }
}