using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Control
{
    [AttributeUsage( AttributeTargets.Method )]
    public class ControlInAttribute : Attribute
    {
        public string Name { get; set; }

        public ControlInAttribute( string name )
        {
            this.Name = name;
        }
    }
}