using KatnisssSpaceSimulator.Control;
using KatnisssSpaceSimulator.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities
{
    public class FActuatorXY : Functionality
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

        public override void Load( JToken data )
        {
            throw new NotImplementedException();
        }

        public override JToken Save()
        {
            throw new NotImplementedException();
        }
    }
}