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
        public FluidState FluidState { get; set; }

        public float MaxVolume { get; set; }

        public float Mass => Contents == null ? 0f : (float)Contents?.GetMass();

        private FlowTank _cachedTank;
        private bool _isStructurallyDirty = true; // Start dirty to force initial build.

        public event IHasMass.MassChange OnAfterMassChanged;

        void FixedUpdate()
        {
            // Optimize - only compute if needed / if not in equilibrium.
            // The inflows and contents and stuff are all known so this can be done
            // Limit flash to tanks whose composition/pressure change significantly.
            //(ISubstanceStateCollection s, FluidState f) = VaporLiquidEquilibrium.ComputeFlash( Contents, FluidState, MaxVolume, TimeManager.FixedDeltaTime );
        }


        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // On a structural rebuild, we create the tank object.
            if( _cachedTank == null )
            {
                _cachedTank = new FlowTank( this.MaxVolume );
            }

            _cachedTank.SetNodes( TriangulationPositions, Inlets );
            if( this.Contents != null )
            {
                _cachedTank.Contents.Clear();
                _cachedTank.Contents.Add( this.Contents );
            }

            c.TryAddFlowObj( this, _cachedTank );

            Vessel vessel = this.transform.GetVessel();
            Transform reference = vessel.ReferenceTransform;

            foreach( var inlet in Inlets )
            {
                Vector3 inletPosInReferenceSpace = reference.InverseTransformPoint( this.transform.TransformPoint( inlet.LocalPosition ) );
                FlowPipe.Port flowInlet = new FlowPipe.Port( (IResourceConsumer)_cachedTank, inletPosInReferenceSpace );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            _isStructurallyDirty = false;
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            // A change in physics vectors is NOT a structural change and should not invalidate the network.
            // A change in geometry (nodes/inlets) or capacity IS a structural change.
            return !_isStructurallyDirty;
        }

        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // This is the correct place for frequent state updates like physics.
            if( _cachedTank != null )
            {
                Vessel vessel = this.transform.GetVessel();
                if( vessel == null ) return;

                Transform reference = vessel.ReferenceTransform;
                INonInertialReferenceFrame vesselFrame = vessel.ReferenceFrameTransform.NonInertialReferenceFrame();
                Vector3Dbl localPosition = (Vector3Dbl)reference.InverseTransformPoint( this.transform.position );
                Vector3Dbl gravityAbsolute = GravityUtils.GetNBodyGravityAcceleration( vessel.ReferenceFrameTransform.AbsolutePosition );
                Vector3Dbl gravityLocal = vesselFrame.InverseTransformDirection( (Vector3)gravityAbsolute );
                Vector3Dbl fictitiousAccel = vesselFrame.GetFicticiousAcceleration( localPosition, Vector3Dbl.zero );
                Vector3Dbl frameAngularVelocity = -vesselFrame.InverseTransformAngularVelocity( Vector3Dbl.zero );
                Vector3Dbl netAcceleration = gravityLocal + fictitiousAccel;

                _cachedTank.FluidAcceleration = (Vector3)netAcceleration;
                _cachedTank.FluidAngularVelocity = (Vector3)frameAngularVelocity;
            }
        }

        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the tank.
            if( snapshot.TryGetFlowObj( this, out FlowTank tankSnapshot ) )
            {
                this.Contents.Clear();
                this.Contents.Add( tankSnapshot.Contents );
            }
        }


        [MapsInheritingFrom( typeof( FResourceContainer_FlowTank ) )]
        public static SerializationMapping FResourceContainer_FlowTankMapping()
        {
            return new MemberwiseSerializationMapping<FResourceContainer_FlowTank>()
                .WithMember( "max_volume", o => o.MaxVolume )
                .WithMember( "triangulation_positions", o => o.TriangulationPositions )
                .WithMember( "inlets", o => o.Inlets )
                .WithMember( "contents", o => o.Contents )
                .WithMember( "fluid_state", o => o.FluidState );
        }
    }
}