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
	public class F3AxisActuator : MonoBehaviour, IPersistent
	{
        // TODO - add actual functionality.

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public float MinZ { get; set; }
        public float MaxZ { get; set; }

        [NamedControl( "Set X" )]
        private ControlleeInput<float> SetX;
        public void OnSetX( float x )
        {
            this.X = x;
        }

        [NamedControl( "Set Y" )]
        private ControlleeInput<float> SetY;
        public void OnSetY( float y )
        {
            this.Y = y;
        }
        
        [NamedControl( "Set Z" )]
        private ControlleeInput<float> SetZ;
        public void OnSetZ( float z )
        {
            this.Z = z;
        }
        
        [NamedControl( "Set XYZ" )]
        private ControlleeInput<Vector3> SetXYZ;
        public void OnSetXYZ( Vector3 xyz )
        {
            this.X = xyz.x;
            this.Y = xyz.y;
            this.Z = xyz.z;
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
