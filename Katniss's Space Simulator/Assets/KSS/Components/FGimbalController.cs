using KSS.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class FGimbalController : MonoBehaviour, IPersistent
    {
        public delegate T ControlParamGetter<T>();
        public delegate void ControlParamSetter<T>( T value );

        // need a way to specify orientation (connect a node).

        // needs a way to specify orientations of individual engines to convert.
        // needs outputs that go into the engines.

        public struct ControlGroup
        {
            // ControlParam is kind of a "reverse" connection. it is on the left/input side, and is used to retrieve a value. So it's really just an control input, but it doesn't get invoked.

            [ControlOut( ControlType.Parameter, "Transform", "Specifies which object to use as the coordinate frame of the controlled engine.\nPick the corresponding engine." )]
            // params shouldn't be events.
            public ControlParamGetter<Transform> GetEngineTransform; // the value of this getter is going to be assigned to a *marked* transform getter method on the engine.

            [ControlOut( ControlType.Action, "Deflection" )]
            public event ControlParamSetter<Vector2> OnSetDeflection; // the value of this is going to be assigned to the control input on the engine.
        }

        [ControlGroup(Size = 5)] // this attribute tells the UI to spawn a group containing either/or/both inputs and outputs.
        private ControlGroup[] ControlledEngines;

        [ControlIn( "set_deflection", "Set Deflection" )]
        private void SetDeflection( Vector2 deflection )
        {
            foreach( var )
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            throw new NotImplementedException();
            // serialize the controlled array, otherwise the stuff is not gonna be set.
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            throw new NotImplementedException();
            // serialize the controlled array, otherwise the stuff is not gonna be set.
        }
    }
}