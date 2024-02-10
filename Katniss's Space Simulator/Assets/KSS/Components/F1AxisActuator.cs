using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class F1AxisActuator : MonoBehaviour, IPersistent
    {
        // TODO - add actual functionality.

        public float X { get; set; }

        public float MinX { get; set; }
        public float MaxX { get; set; }
        
        [NamedControl( "Set X" )]
        private ControlleeInput<float> SetX;
        public void OnSetX( float x )
        {
            this.X = x;
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            throw new NotImplementedException();
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            throw new NotImplementedException();
        }
	}
}