using KSS.Control;
using KSS.Core;
using KSS.Core.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public JToken Save( int fileVersion )
        {
            throw new NotImplementedException();
        }

        public void Load( int fileVersion, JToken data )
        {
            throw new NotImplementedException();
        }
    }
}