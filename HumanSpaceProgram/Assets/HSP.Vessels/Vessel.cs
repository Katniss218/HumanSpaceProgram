using HSP.Core.Mods;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using HSP.Core.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Core
{
	public static class Vessel_Transform_Ex
	{
		/// <summary>
		/// Gets the <see cref="Vessel"/> attached to this transform.
		/// </summary>
		/// <returns>The vessel. Null if the transform is not part of a vessel.</returns>
		public static Vessel GetVessel( this Transform part )
		{
			return part.root.GetComponent<Vessel>();
		}

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this gameobject.
        /// </summary>
        /// <returns>The vessel. Null if the gameobject is not part of a vessel.</returns>
        public static Vessel GetVessel( this GameObject part )
		{
			return GetVessel( part.transform );
		}
	}

    /// <summary>
    /// A vessel is a moving object consisting of a hierarchy of "parts".
    /// </summary>
    /// <remarks>
    /// Vessels exist only in the gameplay scene.
    /// </remarks>
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

		[SerializeField]
		Transform _rootPart;
		public Transform RootPart
		{
			get => _rootPart;
			set
			{
				if( _rootPart != null )
					_rootPart.SetParent( null, true );
				_rootPart = value;
				if( value != null )
					value.SetParent( this.transform, true );
				RecalculatePartCache();
			}
		}

		public IPhysicsObject PhysicsObject { get; private set; }
		public RootObjectTransform RootObjTransform { get; private set; }

		public Transform ReferenceTransform => this.transform;

		// the active vessel has also glithed out and accelerated to the speed of light at least once after jettisonning the side tanks on the pad.

		[field: SerializeField]
		int PartCount { get; set; } = 0;

		// parts with xyz could be modified to be an array, and that array has its callbacks.
		// on separation, parts are recalced fully, but when a part itself changes, that part updates the vessel via the delegate.

		[SerializeField]
		IHasMass[] _partsWithMass;

		[SerializeField]
		Collider[] _partsWithCollider; // TODO - this probably should be a dictionary with type as input, for modding support.

		public Action OnAfterRecalculateParts;

#warning TODO - Accumulatable values - https://github.com/Katniss218/HumanSpaceProgram/issues/19
		/*
        private struct Entry
        {
            // something to recalculate from scratch.
            // something to recalculate a single value (responds to messages sent by compatible components).
            /// <summary>
            /// True if the value can be adjusted and doesn't have to be recalculated from scratch every time.
            /// </summary>
            bool IsTweakable;
        }

        Dictionary<NamespacedIdentifier, object> _cache;
        Dictionary<NamespacedIdentifier, Entry> _cachedProps;
        */

		// mass and colliders

		void Awake()
		{
			this.RootObjTransform = this.GetComponent<RootObjectTransform>();
			this.PhysicsObject = this.GetComponent<IPhysicsObject>();
			this.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
		}

		void Start()
		{
			this.PhysicsObject = this.GetComponent<IPhysicsObject>(); // needs to be here for deserialization, because it might be added in any order and I can't use RequireComponent because it needs to be removed when pinning.
			RecalculatePartCache();
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
			SetPhysicsObjectParameters(); // this full recalc every frame should be replaced by update-based approach.

			Vector3Dbl airfGravityForce = GravityUtils.GetNBodyGravityForce( this.AIRFPosition, PhysicsObject.Mass );

			PhysicsObject.AddForce( (Vector3)airfGravityForce );


			// ---------------------

			// There's also multi-scene physics, which apparently might be used to put the origin of the simulation at 2 different vessels, and have their positions accuratly updated???
			// doesn't seem like that to me reading the docs tho, but idk.
		}

		public void RecalculatePartCache()
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
			_partsWithMass = this.GetComponentsInChildren<IHasMass>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
			_partsWithCollider = this.GetComponentsInChildren<Collider>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
			OnAfterRecalculateParts?.Invoke();
		}

		/// <summary>
		/// Returns the local space center of mass, and the mass [kg] itself.
		/// </summary>
		private (Vector3 localCenterOfMass, float mass, Matrix3x3 inertia) RecalculateMass()
		{
			Vector3 centerOfMass = Vector3.zero;
			float mass = 0;

			List<(float, Vector3)> masses = new();

			foreach( var massivePart in this._partsWithMass )
			{
				Vector3 vesselSpacePosition = this.transform.InverseTransformPoint( massivePart.transform.position );
				centerOfMass += vesselSpacePosition * massivePart.Mass; // potentially precision issues if vessel is far away from origin.
				mass += massivePart.Mass;
				masses.Add( (massivePart.Mass, vesselSpacePosition) );
			}
			if( mass > 0 )
			{
				centerOfMass /= mass;
			}
			Matrix3x3 inertia = InertiaUtils.CalculateInertiaTensor( masses );
			return (centerOfMass, mass, inertia);
		}

		void SetPhysicsObjectParameters()
		{
			(Vector3 comLocal, float mass, Matrix3x3 inertia) = this.RecalculateMass();
			this.PhysicsObject.LocalCenterOfMass = comLocal;
			this.PhysicsObject.Mass = mass;
			//var x = this.PhysicsObject.MomentOfInertiaTensor;

			// disabled for now. needs a better calculation of moments of inertia
			//this.PhysicsObject.MomentOfInertiaTensor = inertia; // this is around an order of magnitude too small in each direction, but that might be because we're assuming point masses.
		}

		public Vector3Dbl AIRFPosition { get => this.RootObjTransform.AIRFPosition; set => this.RootObjTransform.AIRFPosition = value; }
		public QuaternionDbl AIRFRotation { get => this.RootObjTransform.AIRFRotation; set => this.RootObjTransform.AIRFRotation = value; }

		public bool IsPinned { get; private set; }
#warning TODO - this pinning stuff should be an extension inside trajectories.
        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public void Pin( CelestialBody body, Vector3Dbl localPosition, QuaternionDbl localRotation )
		{
			DestroyImmediate( (Component)this.PhysicsObject );
			PinnedPhysicsObject ppo = this.gameObject.AddComponent<PinnedPhysicsObject>();
			ppo.ReferenceBody = body;
			ppo.ReferencePosition = localPosition;
			ppo.ReferenceRotation = localRotation;
			this.PhysicsObject = ppo;
			this.IsPinned = true;
		}

		/// <summary>
		/// Unpins the vessel from a celestial body at its current location.
		/// </summary>
		public void Unpin()
		{
			DestroyImmediate( (Component)this.PhysicsObject );
			this.PhysicsObject = this.gameObject.AddComponent<FreePhysicsObject>();
			this.IsPinned = false;
		}

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