using HSP.CelestialBodies.Surfaces;
using HSP.ReferenceFrames;
using HSP.Time;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A pinned transform that repositions itself and its children whenever it goes too far from scene origin.
    /// </summary>
    public class PinnedLODSphereReferenceFrameTransform : PinnedCelestialBodyReferenceFrameTransform
    {
        private LODQuadSphere _quadSphere;
        /// <summary>
        /// The maximum scene-space position for the LODSphere Quad parent gameobject.
        /// </summary>
        public float MaxPosition { get; set; } = 2000;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            Vector3 scenePosition = this.Position;
#warning TODO - only switch for bodies that are close (within 2x radius) to the center?
            if( scenePosition.magnitude > MaxPosition )
            {
                if( this.gameObject.name.Contains( "main" ) )
                {

                }
                foreach( Transform child in this.transform )
                {
                    child.position += scenePosition;
                }
                this.Position = Vector3.zero;
            }

            //LODQuad.ResetPositionAndRotationAll( _quadSphere ); // this (only partially) fixes the desync, but is slow.

            // with the fix:
            // when one of the quad parents is disabled and reenabled, that quad will start switching at a different time to the quad parents that were not disabled
            // so with 1 quad disabled/reenabled, the flickers occur when that one quad exceeds MaxPosition, as well as when the other quad parents exceed their MaxPositions
            // if world pos of all quad parents is the same, then these will occue simultaneously and no flicker happens.

            // without the fix:
            // also flickers in the same way, but additionally, the old quad parents don't update their positions correctly.
            // flicker is less noticeable because a lot of the quad parents are fucked and misplaced, but still occurs.

            // for the flicker to start occuring, the other quad parents need to shift their positions during the time that our quad is disabled
            // - larger MaxPosition on non-disabled quad parents makes for longer disabled times needed).

            // the quad parent will snap back to its position after being reenabled, but if the other parents updated their pinned positions in the meantime then this will desync?

            // that means that the position is seemingly not calculated correctly...
            // If it was, the position of the quad would be the same regardless of where the parent is (within precision limits ofc).
#warning    CHECK - is the position of the quad in the planet's frame constant? - we need to stop the sphere from rebuilding on liftoff (tie to input key?), otherwise the quad might move/get deleted
            // maybe it's related to the rebuild? hmm, the planet is moving but the quad itself uses old positions or somethin?
            
            // position flickers even without rebuilds and seems shift relative to the planet.

            // after the position switches, the next frame seems to be fucked, and the frame after that is alright again?


            if( this.gameObject.name.Contains( "main" ) )
            {
                var scenepos = this.transform.GetChild( 0 ).position;
                var worldpos = this.SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( scenepos );
                var bodyPos = this.ReferenceTransform.OrientedReferenceFrame().InverseTransformPosition( worldpos );

#warning INFO - timemanager.UT is 0.02 later than the reference frame returned by the scene reference frame provider. which is good, but something is still borked?
                // maybe one thing uses that frame but the other thing doesn't?
                // would be nice to have a getter for the 'next' frame as well.
                //Debug.Log( TimeManager.UT + " : " + scenePosition.magnitude + " : " + (this.ReferencePosition).magnitude );
                Debug.Log( TimeManager.UT + " : " + bodyPos.magnitude );

            }
            // with the fix, the (this.ReferenceTransform.Position - scenePosition).magnitude increases by around 1000 each time the parent is shifted.
            // without the fix, the position is shifted apparently by more than 1000, closer to 2000 but not quite.

#warning TODO - verify whether the quad (mesh) position in the planet's frame is constant.
            // timemanager.UT vs referenceframe.AtUT issues?
            // it doesn't switch to where you'd expect it to, since the change in referenece position is not 2000 units, and the magnitude of that change depends on how fast the scene is moving.
        }

        public const string ADD_PINNED_LOD_REFERENCE_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".addpinnedlreftrans";

        [HSPEventListener( HSPEvent_ON_LOD_QUAD_PARENT_CREATED.ID, ADD_PINNED_LOD_REFERENCE_TRANSFORM )]
        public static void AddPinnedLODSphereReferenceFrameTransform( LODQuadSphere sphere )
        {
            var p = sphere.QuadParent.gameObject.AddComponent<PinnedLODSphereReferenceFrameTransform>();
            p.SceneReferenceFrameProvider = sphere.CelestialBody.ReferenceFrameTransform.SceneReferenceFrameProvider;
            p._quadSphere = sphere;
            Vector3Dbl localPos = Vector3Dbl.zero;
            p.SetReference( sphere.CelestialBody, localPos, Quaternion.identity );
        }

        [MapsInheritingFrom( typeof( PinnedLODSphereReferenceFrameTransform ) )]
        public static SerializationMapping PinnedLODSphereReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<PinnedLODSphereReferenceFrameTransform>()
                .WithMember( "max_position", o => o.MaxPosition );
        }
    }
}