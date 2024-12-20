using HSP.CelestialBodies.Surfaces;
using UnityEngine;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A pinned transform that repositions itself and its children whenever it goes too far from scene origin.
    /// </summary>
    public class PinnedLODSphereReferenceFrameTransform : PinnedReferenceFrameTransform
    {
        public float MaxPosition { get; set; } = 2000;

        protected override void FixedUpdate()
        {
            Vector3 scenePosition = this.Position;

            if( scenePosition.magnitude > MaxPosition )
            {
                foreach( Transform child in this.transform )
                {
                    child.position += scenePosition;
                }
                this.Position = Vector3.zero;
            }

            base.FixedUpdate();
        }

        public const string ADD_PINNED_LOD_REFERENCE_TRANSFORM = HSPEvent.NAMESPACE_HSP + ".addpinnedlreftrans";

        [HSPEventListener( HSPEvent_ON_LOD_QUAD_PARENT_CREATED.ID, ADD_PINNED_LOD_REFERENCE_TRANSFORM )]
        public static void AddPinnedLODSphereReferenceFrameTransform( LODQuadSphere sphere )
        {
            var p = sphere.QuadParent.gameObject.AddComponent<PinnedLODSphereReferenceFrameTransform>();
            Vector3Dbl localPos = Vector3Dbl.zero;
            p.SetReference( sphere.CelestialBody, localPos, Quaternion.identity );
        }
    }
}