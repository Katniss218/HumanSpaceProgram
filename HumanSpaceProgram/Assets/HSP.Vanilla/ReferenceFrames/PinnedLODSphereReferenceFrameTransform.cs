using HSP.CelestialBodies.Surfaces;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A pinned transform that repositions itself and its children whenever it goes too far from scene origin.
    /// </summary>
    public class PinnedLODSphereReferenceFrameTransform : PinnedCelestialBodyReferenceFrameTransform
    {
        /// <summary>
        /// The maximum scene-space position for the LODSphere Quad parent gameobject.
        /// </summary>
        public float MaxPosition { get; set; } = 2000;

        LODQuadSphere _quadSphere;

        protected override void FixedUpdate()
        {
            Vector3 scenePosition = this.Position;
            // TODO - Potentially needs a synchronized approach to prevent flickering. Verify that this can't be fixed by better handling of float precision or fixing any potential bugs in the pinned transform.
            if( scenePosition.magnitude > MaxPosition )
            {
                this.Position = Vector3.zero;
                LODQuad.ResetPositionAndRotationAll( _quadSphere );
            }

            base.FixedUpdate(); // updating after reduces flickering, but doesn't completly eliminate it.
        }

        protected override void OnEnable()
        {
            LODQuad.ResetPositionAndRotationAll( _quadSphere );

            base.OnEnable();
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