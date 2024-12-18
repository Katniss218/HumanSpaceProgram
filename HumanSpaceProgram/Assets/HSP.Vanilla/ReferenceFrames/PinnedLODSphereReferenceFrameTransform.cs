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
    }
}