using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class RectTransformTrackCursor : MonoBehaviour
    {
        /// <summary>
        /// The offset to apply to the position of this object.
        /// </summary>
        public Vector2 Offset { get; set; }

        void LateUpdate()
        {
            Vector2 pos = Input.mousePosition;
            this.transform.position = pos + Offset;
        }
    }
}