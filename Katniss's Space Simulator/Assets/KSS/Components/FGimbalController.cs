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
        public class ControlGroup : Control.ControlGroup
        {
            // Control Parameters are kind of a "reverse" connection. it is on the left/input side, and is used to retrieve a value.
            // So it's really just a control input, but it should never get invoked.

            [NamedControl( "Transform", "Specifies which object to use as the coordinate frame of the controlled engine.\nPick the corresponding engine." )]
            public ControlParameterInput<Transform> GetReferenceTransform; // the value of this getter is going to be assigned to a *marked* transform getter method on the engine.

            [NamedControl( "Deflection" )]
            public ControllerOutput<Vector2> OnSetDeflection; // the value of this is going to be assigned to the control input on the engine.
        }

        [NamedControl( "Controlled Actuators" )]
        private ControlGroup[] ControlledActuators = new ControlGroup[5];

#warning FUCK - here doesn't work
        [NamedControl( "set_deflection", "Set Deflection" )]
        private ControllerInput<Vector2> SetDeflection = new ControllerInput<Vector2>( OnSetDeflection );
        private void OnSetDeflection( Vector2 deflection )
        {
            foreach( var con in ControlledActuators )
            {

            }
            // do stuff...
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
#warning FUCK - here works
            SetDeflection = new ControllerInput<Vector2>( OnSetDeflection );

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