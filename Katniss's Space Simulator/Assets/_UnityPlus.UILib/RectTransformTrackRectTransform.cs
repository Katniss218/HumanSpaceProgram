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

        public Vector2 Offset { get; set; }

        public Vector2 TargetCorner { get; set; }
        public Vector2 SelfCorner { get; set; }

        void LateUpdate()
        {
            RectTransform self = (RectTransform)transform;
            Rect targetRect = Target.rect;

            Vector2 targetCornerPos = Target.TransformPoint( new Vector2( targetRect.width * (TargetCorner.x - Target.pivot.x), targetRect.height * (TargetCorner.y - Target.pivot.y ) ) );

            self.pivot = SelfCorner;
            
            this.transform.position = targetCornerPos + Offset;
        }
    }
}