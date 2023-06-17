using KSS.Control;
using KSS.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Functionalities
{
    public class FActuatorXY : MonoBehaviour, IPersistent
    {
        // 2-axis actuator.

        // reference direction = parent direction.

        /// <summary>
        /// The transform used as a reference (0,0) orientation.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceTransform { get; set; }

        [field: SerializeField]
        public Transform ActuatorTransform { get; set; }

        private float _x;
        private float _y;

        // min/max ranges.

        [ControlIn( "set.x", "Set X" )]
        public void SetX( float x )
        {
            this._x = x;
        }

        [ControlIn( "set.y", "Set Y" )]
        public void SetY( float y )
        {
            this._y = y;
        }

        void Update()
        {
            // todo.
        }

        public void SetData( Loader l, SerializedData data )
        {
            throw new NotImplementedException();
        }

        public SerializedData GetData( Saver s )
        {
            throw new NotImplementedException();
        }
    }
}