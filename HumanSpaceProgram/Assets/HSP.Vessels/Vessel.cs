using HSP.ReferenceFrames;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityPlus.Serialization;
// using UnityPlus.Serialization.Descriptors;
// using Ctx = UnityPlus.Serialization.Ctx;

// namespace HSP.Vessels
// {
//     public static class HSPEvent_AFTER_VESSEL_CREATED
//     {
//         public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_created.after";
//     }

//     public static class HSPEvent_AFTER_VESSEL_DESTROYED
//     {
//         public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_destroyed.after";
//     }

//     public static class HSPEvent_AFTER_VESSEL_SPLIT
//     {
//         public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_split.after";
//         public struct Data
//         {
//             /// <summary>
//             /// The original vessel from which parts were split.
//             /// </summary>
//             public Vessel OldVessel;
//             /// <summary>
//             /// The new vessel created from the split.
//             /// </summary>
//             public Vessel NewVessel;

//             public IReadOnlyList<VesselPart> SplitParts;
//         }
//     }

//     public static class HSPEvent_AFTER_VESSEL_MERGE
//     {
//         public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_merge.after";
//         public struct Data
//         {
//             /// <summary>
//             /// The vessel that survived the merge.
//             /// </summary>
//             public Vessel RemainingVessel;
//             /// <summary>
//             /// The vessel that was merged into RemainingVessel and destroyed.
//             /// </summary>
//             public Vessel MergedVessel;

//             /// <summary>
//             /// The list of parts that were merged into RemainingVessel.
//             /// </summary>
//             public IReadOnlyList<VesselPart> MergedParts;
//         }
//     }

//     public static class HSPEvent_AFTER_VESSEL_HIERARCHY_CHANGED
//     {
//         public const string ID = "caa42be2-5b08-4a27-a35e-bec2b7aca5e3";
//     }

//     /// <summary>
//     /// A vessel is a moving object consisting of a hierarchy of "parts".
//     /// </summary>
//     /// <remarks>
//     /// Vessels exist only in the gameplay scene.
//     /// </remarks>
//     public sealed partial class Vessel : MonoBehaviour, IVessel
//     {
//         [SerializeField]
//         private string _displayName;
//         public string DisplayName
//         {
//             get => _displayName;
//             set { _displayName = value; this.gameObject.name = value; }
//         }

//         IPhysicsTransform _physicsTransform;
//         public IPhysicsTransform PhysicsTransform
//         {
//             get
//             {
//                 if( _physicsTransform.IsUnityNull() )
//                     _physicsTransform = this.GetComponent<IPhysicsTransform>();
//                 return _physicsTransform;
//             }
//         }

//         IReferenceFrameTransform _referenceFrameTransform;
//         public IReferenceFrameTransform ReferenceFrameTransform
//         {
//             get
//             {
//                 if( _referenceFrameTransform.IsUnityNull() )
//                     _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
//                 return _referenceFrameTransform;
//             }
//         }

//         public Transform ReferenceTransform => this.transform;

//         // the active vessel has also glithed out and accelerated to the speed of light at least once after jettisonning the side tanks on the pad.

//         // parts with xyz could be modified to be an array, and that array has its callbacks.
//         // on separation, parts are recalced fully, but when a part itself changes, that part updates the vessel via the delegate.

//         VesselPart[] _parts;
//         VesselIsland[] _islands;
//         VesselAttachmentGraph _attachments;

//         public IReadonlyVesselAttachmentGraph Attachments => _attachments;
//         public IEnumerable<IReadonlyVesselIsland> Islands => _islands;

//         public IEnumerable<VesselPart> Parts => _parts;

//         // mass and colliders

//         void Awake()
//         {
//             VesselManager.Register( this );
//         }

//         void Start()
//         {
//             this.RecalculatePartCache();
//             this.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

//             HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_CREATED.ID, this );
//             this.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
//         }

//         private void OnDestroy()
//         {
//             try
//             {
//                 VesselManager.Unregister( this );
//             }
//             catch( SingletonInstanceException )
//             {
//                 // OnDisable was called when scene was unloaded, ignore.
//             }

//             HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_DESTROYED.ID, this );
//         }

//         void FixedUpdate()
//         {
//             //if( this._rootPart.gameObject.activeSelf )
//             //{
//             //    this.SetPhysicsObjectParameters(); // this full recalc every frame should be replaced by update-based approach.

//             //    if( this.ReferenceFrameTransform.GetPosition().magnitude > 1e5 )
//             //        this._rootPart.gameObject.SetActive( false );
//             //}
//             //if( !this._rootPart.gameObject.activeSelf && this.ReferenceFrameTransform.GetPosition().magnitude <= 1e5 )
//             //{
//             //    this._rootPart.gameObject.SetActive( true );
//             //}
//             // Replace with vessel unloading (though externally to this class).

//             // ---------------------

//             // There's also multi-scene physics, which apparently might be used to put the origin of the simulation at 2 different vessels, and have their positions accuratly updated???
//             // doesn't seem like that to me reading the docs tho, but idk.
//         }

//         private Transform GetOrCreateIslandTransform( int index )
//         {
//             string islandName = $"Island_{index}";
//             Transform child = this.transform.Find( islandName );
//             if( child == null )
//             {
//                 GameObject islandGO = new GameObject( islandName );
//                 child = islandGO.transform;
//                 child.SetParent( this.transform, false );
//             }
//             return child;
//         }

//         private void CleanupExtraIslandTransforms( int activeCount )
//         {
//             for( int i = this.transform.childCount - 1; i >= 0; i-- )
//             {
//                 Transform child = this.transform.GetChild( i );
//                 if( child.name.StartsWith( "Island_" ) )
//                 {
//                     if( int.TryParse( child.name.Substring( 7 ), out int index ) )
//                     {
//                         if( index >= activeCount )
//                         {
//                             while( child.childCount > 0 )
//                             {
//                                 child.GetChild( 0 ).SetParent( this.transform, true );
//                             }
//                             Destroy( child.gameObject );
//                         }
//                     }
//                 }
//             }
//         }

//         public void RebuildIslands()
//         {
//             if( _attachments == null )
//             {
//                 _islands = Array.Empty<VesselIsland>();
//                 CleanupExtraIslandTransforms( 0 );
//                 return;
//             }

//             var detectedIslands = VesselAttachmentGraph.DetectIslands( _attachments );
//             _islands = detectedIslands.ToArray();

//             for( int i = 0; i < _islands.Length; i++ )
//             {
//                 var island = _islands[i];
//                 Transform islandTransform = GetOrCreateIslandTransform( i );
//                 foreach( var part in island.Parts )
//                 {
//                     if( part != null && part.transform.parent != islandTransform )
//                     {
//                         part.transform.SetParent( islandTransform, true );
//                     }
//                 }
//             }

//             CleanupExtraIslandTransforms( _islands.Length );
//         }

//         public void SetGraph( VesselAttachmentGraph graph, VesselPart[] newParts = null )
//         {
//             _attachments = graph;
//             _parts = graph != null ? graph.Nodes.ToArray() : Array.Empty<VesselPart>();

//             foreach( var part in _parts )
//             {
//                 if( part != null )
//                 {
//                     part.Vessel = this;
//                 }
//             }

//             RebuildIslands();
//             RecalculatePartCache();
//         }

//         public void RecalculatePartCache()
//         {
//             //if( _parts == null )
//             //{
//             //    _partsWithMass = new IHasMass[] { };
//             //    _partsWithCollider = new Collider[] { };
//             //    return;
//             //}

//             //_partsWithMass = this.GetComponentsInChildren<IHasMass>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
//             //_partsWithCollider = this.GetComponentsInChildren<Collider>(); // GetComponentsInChildren might be slower than custom methods? (needs testing)
//         }

//         /// <summary>
//         /// Returns the local space center of mass, and the mass [kg] itself.
//         /// </summary>
//         //private (Vector3 localCenterOfMass, float mass, Matrix3x3 inertia) RecalculateMass()
//         //{
//         //    Vector3 centerOfMass = Vector3.zero;
//         //    float mass = 0;

//         //    List<(float, Vector3)> masses = new();

//         //    foreach( var massivePart in this._partsWithMass )
//         //    {
//         //        Vector3 vesselSpacePosition = this.transform.InverseTransformPoint( massivePart.transform.position );
//         //        centerOfMass += vesselSpacePosition * massivePart.Mass; // potentially precision issues if vessel is far away from origin.
//         //        mass += massivePart.Mass;
//         //        masses.Add( (massivePart.Mass, vesselSpacePosition) );
//         //    }
//         //    if( mass > 0 )
//         //    {
//         //        centerOfMass /= mass;
//         //    }
//         //    Matrix3x3 inertia = InertiaUtils.CalculateInertiaTensor( masses );
//         //    return (centerOfMass, mass, inertia);
//         //}

//         //void SetPhysicsObjectParameters()
//         //{
//         //    //(Vector3 comLocal, float mass, Matrix3x3 inertia) = this.RecalculateMass();
//         //    this.PhysicsTransform.LocalCenterOfMass = comLocal;
//         //    this.PhysicsTransform.Mass = mass;
//         //    //var x = this.PhysicsObject.MomentOfInertiaTensor;

//         //    // disabled for now. needs a better calculation of moments of inertia
//         //    //this.PhysicsObject.MomentOfInertiaTensor = inertia; // this is around an order of magnitude too small in each direction, but that might be because we're assuming point masses.
//         //}

//         /// <summary>
//         /// Calculates the scene world-space point at the very bottom of the vessel. Useful when placing it at launchsites and such.
//         /// </summary>
//         public Vector3 GetBottomPosition()
//         {
//             throw new NotImplementedException();
//             // TODO - compute bounds when attaching parts?
//         }


//         // -=-=-=-=-=-=-=-


//         private void OnDrawGizmos()
//         {
//             Gizmos.color = Color.blue;
//             Gizmos.DrawWireCube( this.transform.TransformPoint( this.PhysicsTransform.LocalCenterOfMass ), Vector3.one * 0.25f );
//         }

//         public static double GetExhaustVelocity( (Vector3 thrust, float exhaustVelocity)[] thrusters )
//         {
//             Vector3 totalThrust = Vector3.zero;
//             float totalMassFlow = 0.0f;

//             foreach( (var thrust, var exhaustVelocity) in thrusters )
//             {
//                 totalThrust += thrust;
//                 totalMassFlow += thrust.magnitude * exhaustVelocity;
//             }

//             return totalThrust.magnitude / totalMassFlow;
//         }

//         public static double GetDeltaV( double exhaustVelocity, double initialMass, double finalMass )
//         {
//             return exhaustVelocity * Math.Log( initialMass / finalMass );
//         }

//         /// <summary>
//         /// Calculates the initial mass required for a vehicle to achieve a given delta-V.
//         /// </summary>
//         /// <param name="deltaV">The desired delta-V, in [m/s].</param>
//         /// <param name="exhaustVelocity">The effective exhaust velocity, in [m/s].</param>
//         /// <param name="finalMass">The final mass of the vehicle after the burn, in [kg].</param>
//         /// <returns>The initial mass, in [kg].</returns>
//         public static double GetInitialMass( double deltaV, double exhaustVelocity, double finalMass )
//         {
//             return finalMass * Math.Exp( deltaV / exhaustVelocity );
//         }


//         [MapsInheritingFrom( typeof( Vessel ) )]
//         public static IDescriptor VesselMapping()
//         {
//             return new MemberwiseDescriptor<Vessel>()
//                 .WithMember( "display_name", o => o.DisplayName );
//         }
//     }
// }