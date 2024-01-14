using KSS.Control;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class F2AxisActuator : MonoBehaviour, IPersistent
    {
        // 2-axis actuator.

        // reference direction = parent direction.

        /// <summary>
        /// The transform used as a reference (0,0) orientation.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceTransform { get; set; }

        [ControlIn( ControlType.Parameter, "Coordinate Space Transform" )]
        public Transform GetTransform()
        {

        }

        [field: SerializeField]
        public Transform ActuatorTransform { get; set; }

        private float _x;
        private float _y;

        // min/max ranges.

        [ControlIn( ControlType.Action, "Set X" )]
        public void SetX( float x )
        {
            this._x = x;
        }

        [ControlIn( ControlType.Action, "Set Y" )]
        public void SetY( float y )
        {
            this._y = y;
        }

        void Update()
        {
            // todo.
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