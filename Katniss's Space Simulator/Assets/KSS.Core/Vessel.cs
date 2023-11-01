using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using KSS.Core.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KSS.Core
{
    public static class VesselEx
    {
        public static bool IsRootOfVessel( this Transform part )
        {
            if( part.root != part.parent )
                return false;
            Vessel v = part.parent.GetComponent<Vessel>();
            if( v == null )
                return false;
            return v.RootPart == part;
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The vessel. Null if the transform is not part of a vessel.</returns>
        public static Vessel GetVessel( this Transform part )
        {
            return part.root.GetComponent<Vessel>();
        }
    }

    /// <summary>
    /// A vessel is a moving object consisting of a hierarchy of "parts".
    /// </summary>
    [RequireComponent( typeof( PhysicsObject ) )]
    [RequireComponent( typeof( RootObjectTransform ) )]
    public sealed partial class Vessel : MonoBehaviour, IPartObject
    {
        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        [field: SerializeField]
        public Transform RootPart { get; private set; }

        public PhysicsObject PhysicsObject { get; private set; }
        public RootObjectTransform RootObjTransform { get; private set; }

        /// <remarks>
        /// DO NOT USE. This is for internal use, and can produce an invalid state. Use <see cref="VesselHierarchyUtils.SetParent(Transform, Transform)"/> instead.
        /// </remarks>
        [Obsolete( "This is for internal use, and can produce an invalid state." )]
        internal void SetRootPart( Transform part )
        {
            if( part != null && part.GetVessel() != this )
            {
                throw new ArgumentException( $"Can't set the part '{part}' from vessel '{part.GetVessel()}' as root. The part is not a part of this vessel." );
            }

            RootPart = part;
        }

#warning TODO - Vessels' position sometimes glitches out when far away from the origin. Setting the rigidbody to kinematic fixes the issue, which suggests that it is caused by a collision response.
        // Possibly a response to newly unsubdivided planet LOD quad.


        // the active vessel has also glithed out and accelerated to the speed of light at least once after jettisonning the side tanks on the pad.

        [field: SerializeField]
        int PartCount { get; set; } = 0;
        [SerializeField]
        IHasMass[] _partsWithMass;

        [SerializeField]
        Collider[] _partsWithCollider; // TODO - this probably should be a dictionary with type as input, for modding support.

        public event Action OnAfterRecalculateParts;

        public void RecalculateParts()
        {
            if( RootPart == null )
            {
                PartCount = 0;
                _partsWithMass = new IHasMass[] { };
                _partsWithCollider = new Collider[] { };
                OnAfterRecalculateParts?.Invoke();
                return;
            }

            int count = 0;
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push( RootPart );

            while( stack.Count > 0 )
            {
                Transform part = stack.Pop();
                count++;

                foreach( Transform childPart in part )
                {
                    stack.Push( childPart );
                }
            }

            PartCount = count;
            _partsWithMass = RootPart.GetComponentsInChildren<IHasMass>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
            _partsWithCollider = RootPart.GetComponentsInChildren<Collider>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
            OnAfterRecalculateParts?.Invoke();
        }

        /// <summary>
        /// Returns the local space center of mass, and the mass [kg] itself.
        /// </summary>
        public (Vector3 localCenterOfMass, float mass) RecalculateMass()
        {
            Vector3 centerOfMass = Vector3.zero;
            float mass = 0;
            foreach( var massivePart in this._partsWithMass )
            {
                centerOfMass += this.transform.InverseTransformPoint( massivePart.transform.position ) * massivePart.Mass; // potentially precision issues if vessel is far away from origin.
                mass += massivePart.Mass;
            }
            if( mass > 0 )
            {
                centerOfMass /= mass;
            }
            return (centerOfMass, mass);
        }

        public Vector3Dbl AIRFPosition { get => this.RootObjTransform.AIRFPosition; set => this.RootObjTransform.AIRFPosition = value; }
        public QuaternionDbl AIRFRotation { get => this.RootObjTransform.AIRFRotation; set => this.RootObjTransform.AIRFRotation = value; }

        /// <summary>
        /// Calculates the scene world-space point at the very bottom of the vessel. Useful when placing it at launchsites and such.
        /// </summary>
        public Vector3 GetBottomPosition()
        {
            Vector3 dir = this.transform.position - (this.transform.up * 500f); // can bug out for large vessels. need to take the point relative to the center and size of the currently checked collider.
            Vector3 min = this.transform.position;
            float minDist = float.MaxValue;
            foreach( var collider in _partsWithCollider )
            {
                Vector3 closestBound = collider.ClosestPointOnBounds( dir );
                float dst = Vector3.Distance( dir, closestBound );
                if( dst < minDist )
                {
                    minDist = dst;
                    min = closestBound;
                }
            }
            return min;
        }

        void Awake()
        {
            this.RootObjTransform = this.GetComponent<RootObjectTransform>();
            this.PhysicsObject = this.GetComponent<PhysicsObject>();
        }

        void SetPhysicsObjectParameters()
        {
            (Vector3 comLocal, float mass) = this.RecalculateMass();
            this.PhysicsObject.LocalCenterOfMass = comLocal;
            this.PhysicsObject.Mass = mass;
        }

        void Start()
        {
            RecalculateParts();
            //SetPhysicsObjectParameters();
        }

        void OnEnable()
        {
            VesselManager.Register( this );
        }

        void OnDisable()
        {
            try
            {
                VesselManager.Unregister( this );
            }
            catch( InvalidSceneManagerException )
            {
                // scene unloaded.
            }
        }

        void FixedUpdate()
        {
            SetPhysicsObjectParameters();

            Vector3Dbl airfGravityForce = GravityUtils.GetGravityForce( this.AIRFPosition, PhysicsObject.Mass );

            PhysicsObject.AddForce( (Vector3)airfGravityForce );


            // ---------------------

            // There's also multi-scene physics, which apparently might be used to put the origin of the simulation at 2 different vessels, and have their positions accuratly updated???
            // doesn't seem like that to me reading the docs tho, but idk.
        }


        // -=-=-=-=-=-=-=-


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube( this.transform.TransformPoint( this.PhysicsObject.LocalCenterOfMass ), Vector3.one * 0.25f );
        }

        public static double GetExhaustVelocity( (Vector3 thrust, float exhaustVelocity)[] thrusters )
        {
            Vector3 totalThrust = Vector3.zero;
            float totalMassFlow = 0.0f;

            foreach( (var thrust, var exhaustVelocity) in thrusters )
            {
                totalThrust += thrust;
                totalMassFlow += thrust.magnitude * exhaustVelocity;
            }

            return totalThrust.magnitude / totalMassFlow;
        }

        public static double GetDeltaV( double exhaustVelocity, double initialMass, double finalMass )
        {
            return exhaustVelocity * Math.Log( initialMass / finalMass );
        }

        /// <summary>
        /// Calculates the initial mass required for a vehicle to achieve a given delta-V.
        /// </summary>
        /// <param name="deltaV">The desired delta-V, in [m/s].</param>
        /// <param name="exhaustVelocity">The effective exhaust velocity, in [m/s].</param>
        /// <param name="finalMass">The final mass of the vehicle after the burn, in [kg].</param>
        /// <returns>The initial mass, in [kg].</returns>
        public static double GetInitialMass( double deltaV, double exhaustVelocity, double finalMass )
        {
            return finalMass * Math.Exp( deltaV / exhaustVelocity );
        }
    }
}