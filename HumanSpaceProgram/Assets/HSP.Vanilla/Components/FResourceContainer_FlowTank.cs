using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.ResourceFlow;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceContainer_FlowTank : MonoBehaviour, IBuildsFlowNetwork
    {
        /// <summary>
        /// Gets or sets the initial triangulation positions for the tank.
        /// </summary>
        public Vector3[] TriangulationPositions { get; set; }

        public ResourceInlet[] Inlets { get; set; }
        public SubstanceStateCollection Contents { get; set; }

        public float MaxVolume { get; set; }


        FlowTank _cachedTank;


        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the tank?

            Vessel vessel = this.transform.GetVessel();
            Transform reference = vessel.ReferenceTransform;


            // only add if something attaches to the tank that is open.
            if( _cachedTank == null )
            {
                var vesselFrame = vessel.ReferenceFrameTransform.NonInertialReferenceFrame();
                Vector3Dbl airfAcceleration = GravityUtils.GetNBodyGravityAcceleration( vessel.ReferenceFrameTransform.AbsolutePosition );
                Vector3 sceneAcceleration = vessel.ReferenceFrameTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformDirection( (Vector3)airfAcceleration );
                Vector3 vesselAcceleration = vessel.ReferenceFrameTransform.Acceleration;
#warning TODO - non-inertial fake accelerations (centrifugal, coriolis, etc).

                // acceleration due to external forces (gravity) minus the acceleration of the vessel.
                sceneAcceleration -= vesselAcceleration;

                // make tank.
                _cachedTank = new FlowTank( this.MaxVolume );
                _cachedTank.SetNodes( TriangulationPositions, Inlets );
                _cachedTank.Contents = this.Contents;
#warning TODO - define simulation space. should be vessel space.
                _cachedTank.FluidAcceleration = sceneAcceleration;
                //_cachedTank.FluidAngularVelocity = angularVelocity;
            }

            // TODO - validate that something is connected to it. We can use a similar 'connection' structure to control inputs.
            _cachedTank.DistributeFluids(); // settle the fluids so the values at each inlet are correct.
            c.TryAddFlowObj( this, _cachedTank );
            foreach( var inlet in Inlets )
            {
                Vector3 inletPosInReferenceSpace = reference.InverseTransformPoint( this.transform.TransformPoint( inlet.LocalPosition ) );
                FlowPipe.Port flowInlet = new FlowPipe.Port( (IResourceConsumer)_cachedTank, inletPosInReferenceSpace );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
#warning TODO - implement.
            return false;
        }

        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the tank.
            if( snapshot.TryGetFlowObj( _cachedTank, out FlowTank tankSnapshot ) )
            {
                this.Contents = tankSnapshot.Contents;
            }
        }


        [MapsInheritingFrom( typeof( FResourceContainer_FlowTank ) )]
        public static SerializationMapping FResourceContainer_FlowTankMapping()
        {
            return new MemberwiseSerializationMapping<FResourceContainer_FlowTank>()
                .WithMember( "triangulation_positions", o => o.TriangulationPositions )
                .WithMember( "inlets", o => o.Inlets )
                .WithMember( "contents", o => o.Contents );
        }
    }
}