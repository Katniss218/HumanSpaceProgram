using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class RectTransformTrackRectTransform : MonoBehaviour
    {
        /// <summary>
        /// The RectTransform to follow the position of.
        /// </summary>
        public RectTransform Target { get; set; }

        /// <summary>
        /// The offset to apply to the position of this object.
        /// </summary>
        public Vector2 Offset { get; set; }

        void LateUpdate()
        {
            Vector2 pos = Target.TransformPoint( Vector2.zero );
            this.transform.position = pos + Offset;
        }
    }
}