using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.ResourceFlow;
using HSP.Time;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceContainer_FlowTank : MonoBehaviour, IBuildsFlowNetwork, IResourceContainer
    {
        /// <summary>
        /// Gets or sets the initial triangulation positions for the tank.
        /// </summary>
        public Vector3[] TriangulationPositions { get; set; }

        public ResourceInlet[] Inlets { get; set; }
        public ISubstanceStateCollection Contents { get; set; }
        public FluidState State { get; set; }

        public float MaxVolume { get; set; }

        public float Mass => throw new System.NotImplementedException();

        FlowTank _cachedTank;

        public event IHasMass.MassChange OnAfterMassChanged;

        void FixedUpdate()
        {
            // Optimize - only compute if needed / if not in equilibrium.
            // The inflows and contents and stuff are all known so this can be done
            // Limit flash to tanks whose composition/pressure change significantly.
            (ISubstanceStateCollection s, FluidState f) = VaporLiquidEquilibrium.ComputeFlash( Contents, State, MaxVolume, TimeManager.FixedDeltaTime );
        }


        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the tank?

            Vessel vessel = this.transform.GetVessel();
            Transform reference = vessel.ReferenceTransform;


            // only add if something attaches to the tank that is open.
            if( _cachedTank == null )
            {
                INonInertialReferenceFrame vesselFrame = vessel.ReferenceFrameTransform.NonInertialReferenceFrame();

                Vector3Dbl localPosition = (Vector3Dbl)reference.InverseTransformPoint( this.transform.position );

                Vector3Dbl gravityAbsolute = GravityUtils.GetNBodyGravityAcceleration( vessel.ReferenceFrameTransform.AbsolutePosition );

                Vector3Dbl gravityLocal = vesselFrame.InverseTransformDirection( (Vector3)gravityAbsolute );

                Vector3Dbl fictitiousAccel = vesselFrame.GetFicticiousAcceleration( localPosition, Vector3Dbl.zero );

                Vector3Dbl frameAngularVelocity = -vesselFrame.InverseTransformAngularVelocity( Vector3Dbl.zero );

                Vector3Dbl netAcceleration = gravityLocal + fictitiousAccel;

                _cachedTank = new FlowTank( this.MaxVolume );
#warning TODO - define simulation space. should be vessel space.
                _cachedTank.SetNodes( TriangulationPositions, Inlets );
                _cachedTank.Contents = this.Contents;

                // Apply calculated physics vectors (Converted to Unity Single Precision for the solver)
                _cachedTank.FluidAcceleration = (Vector3)netAcceleration;
                _cachedTank.FluidAngularVelocity = (Vector3)frameAngularVelocity;
            }

            // TODO - validate that something is connected to it. We can use a similar 'connection' structure to control inputs.
          //  _cachedTank.DistributeContents(); // settle the fluids so the values at each inlet are correct.
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