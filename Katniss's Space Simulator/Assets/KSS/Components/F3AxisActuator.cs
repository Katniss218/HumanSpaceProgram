using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace Assets.KSS.Components
{
    [Obsolete("Not implemented yet.")] // TODO - add actual functionality.
	public class F3AxisActuator : MonoBehaviour, IPersistsData
	{
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
		[field: SerializeField]
        public float MinX { get; set; } = -5f;
		[field: SerializeField]
        public float MaxX { get; set; } = 5f;
		[field: SerializeField]
        public float MinY { get; set; } = -5f;
		[field: SerializeField]
        public float MaxY { get; set; } = 5f;
		[field: SerializeField]
        public float MinZ { get; set; } = -5f;
		[field: SerializeField]
        public float MaxZ { get; set; } = 5f;

        [NamedControl( "Deflection (X)" )]
        public ControlleeInput<float> SetX;
        private void SetXListener( float x )
        {
            this.X = x;
        }

        [NamedControl( "Deflection (Y)" )]
        public ControlleeInput<float> SetY;
        private void SetYListener( float y )
        {
            this.Y = y;
        }
        
        [NamedControl( "Deflection (Z)" )]
        public ControlleeInput<float> SetZ;
        private void SetZListener( float z )
        {
            this.Z = z;
        }
        
        [NamedControl( "Deflection (XYZ)" )]
        public ControlleeInput<Vector3> SetXYZ;
        private void SetXYZListener( Vector3 xyz )
        {
            this.X = xyz.x;
            this.Y = xyz.y;
            this.Z = xyz.z;
        }

		public SerializedData GetData( IReverseReferenceMap s )
		{
			throw new NotImplementedException();
		}

		public void SetData( SerializedData data, IForwardReferenceMap l )
		{
			throw new NotImplementedException();
		}
	}
}
