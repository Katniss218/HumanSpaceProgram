using KatnisssSpaceSimulator.Core;
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

        [field: SerializeField]
        public Transform ThrustTransform { get; set; }

        /// <summary>
        /// Returns the actual thrust at this moment in time.
        /// </summary>
        public float GetThrust()
        {
            return this.MaxThrust * Throttle;
        }

        void FixedUpdate()
        {
            Part.Vessel.PhysicsObject.AddForceAtPosition( this.ThrustTransform.forward * GetThrust(), this.ThrustTransform.position );
        }

        void Start()
        {
        }

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.W ) )
            {
                Throttle = Throttle > 0.5f ? 0.0f : 1.0f;
            }
        }
    }
}