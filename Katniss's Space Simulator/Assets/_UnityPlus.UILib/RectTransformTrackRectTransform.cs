using System;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class RectTransformTrackRectTransform : MonoBehaviour
    {
        /// <summary>
        /// The RectTransform to follow the center of.
        /// </summary>
        public RectTransform Target { get; set; }

        /// <summary>
        /// Gets or sets the additional offset to apply to this transform.
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Gets or sets the corner of the target transform that will be aligned with this transform.
        /// </summary>
        public Vector2 TargetCorner { get; set; }

        /// <summary>
        /// Gets or sets the corner of this transform that will be aligned with the target.
        /// </summary>
        public Vector2 SelfCorner { get; set; }

        void LateUpdate()
        {
            // Put the two corners 'on top of each other', and translate self by the offset.

            RectTransform self = (RectTransform)transform;
            Rect targetRect = Target.rect;

            Vector2 targetCornerPos = Target.TransformPoint( new Vector2( targetRect.width * (TargetCorner.x - Target.pivot.x), targetRect.height * (TargetCorner.y - Target.pivot.y) ) );

            self.pivot = SelfCorner;

            this.transform.position = targetCornerPos + Offset;
        }
    }
}