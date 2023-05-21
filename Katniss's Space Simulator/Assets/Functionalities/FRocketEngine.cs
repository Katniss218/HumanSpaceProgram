using KatnisssSpaceSimulator.Control;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
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
    public class FRocketEngine : MonoBehaviour, IResourceConsumer
    {
        /// <summary>
        /// The maximum thrust of the engine, in [N].
        /// </summary>
        [field: SerializeField]
        public float MaxThrust { get; set; } = 10000f;

        /// <summary>
        /// Specific impulse of the engine, in [s].
        /// </summary>
        [field: SerializeField]
        public float Isp { get; set; } = 100f; // TODO - curve based on atmospheric pressure.

        /// <summary>
        /// Maximum mass flow (at max thrust), in [kg/s]
        /// </summary>
        public float MaxMassFlow => MaxThrust * 9.80665f / Isp;

        [field: SerializeField]
        public float Throttle { get; set; }

        /// <summary>
        /// Defines which way the engine thrusts (thrust is applied along its `forward` (positive) axis).
        /// </summary>
        [field: SerializeField]
        public Transform ThrustTransform { get; set; }

        [field: SerializeField]
        public SubstanceStateCollection Inflow { get; private set; } = SubstanceStateCollection.Empty;

        Part _part;

        /// <summary>
        /// Returns the actual thrust at this moment in time.
        /// </summary>
        public float GetThrust( float massFlow )
        {
            return massFlow * this.Isp * Throttle;
        }

        /*[ControlIn( "set.throttle", "Set Throttle" )]
        public void SetThrottle( float value )
        {
            this.Throttle = value;
        }*/

        private void Awake()
        {
            _part = this.GetComponent<Part>();
            if( _part == null )
            {
                Destroy( this );
                throw new InvalidOperationException( $"{nameof( FRocketEngine )} can only be added to a part." );
            }
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
                this._part.Vessel.PhysicsObject.AddForceAtPosition( this.ThrustTransform.forward * GetThrust( Inflow.GetMass() ), this.ThrustTransform.position );
            }
        }

        public void ClampIn( SubstanceStateCollection inflow, float dt )
        {
            FlowUtils.ClampMaxMassFlow( inflow, Inflow.GetMass(), MaxMassFlow * Throttle );
        }

        public FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea )
        {
            return FluidState.Vacuum; // temp, inlet condition.
        }

        public void Load( JToken data )
        {
            throw new NotImplementedException();
        }

        public JToken Save()
        {
            throw new NotImplementedException();
        }
    }
}