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
    [Obsolete("Not implemented yet.")] // TODO - add actual functionality.
    public class F1AxisActuator : MonoBehaviour, IPersistsData
    {
        public float X { get; set; }
        
		[field: SerializeField]
        public float MinX { get; set; } = -5f;
		[field: SerializeField]
        public float MaxX { get; set; } = 5f;
        
        [NamedControl( "Deflection (X)" )]
        public ControlleeInput<float> SetX;
        private void SetXListener( float x )
        {
            this.X = x;
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)Persistent_Behaviour.GetData( this, s );

            //ret.AddAll( new SerializedObject()

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            Persistent_Behaviour.SetData( this, data, l );

            //
        }
	}
}