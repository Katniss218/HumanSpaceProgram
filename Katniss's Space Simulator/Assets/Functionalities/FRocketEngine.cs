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
    [Serializable]
    public class FRocketEngine : Functionality
    {
        [field: SerializeField]
        public float MaxThrust { get; set; }

        [field: SerializeField]
        public float Throttle { get; set; }

        /// <summary>
        /// Defines which way the engine thrusts (thrust is applied along its `forward` (positive) axis).
        /// </summary>
        [field: SerializeField]
        public Transform ThrustTransform { get; set; }

        Part _part;

        /// <summary>
        /// Returns the actual thrust at this moment in time.
        /// </summary>
        public float GetThrust()
        {
            return this.MaxThrust * Throttle;
        }

        [ControlIn( "set.throttle", "Set Throttle" )]
        public void SetThrottle( float value )
        {
            this.Throttle = value;
        }

        private void Awake()
        {
            _part = this.GetComponent<Part>();
        }

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.W ) )
            {
                Throttle = Throttle > 0.5f ? 0.0f : 1.0f;
            }
        }

        void FixedUpdate()
        {
            if( this.Throttle > 0.0f )
            {
                this._part.Vessel.PhysicsObject.AddForceAtPosition( this.ThrustTransform.forward * GetThrust(), this.ThrustTransform.position );
            }
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