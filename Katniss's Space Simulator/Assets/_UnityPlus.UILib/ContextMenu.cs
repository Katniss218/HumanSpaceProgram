using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityPlus.UILib
{
    public class ContextMenu : EventTrigger
    {
        /// <summary>
        /// The context menu will track this RectTransform.
        /// </summary>
        public RectTransform Target { get; set; }

        /// <summary>
        /// The offset to apply to the position of this context menu.
        /// </summary>
        public Vector2 Offset { get; set; }

        void LateUpdate()
        {
            Vector2 pos = Target.TransformPoint( Vector2.zero );
            this.transform.position = pos + Offset;
        }

        public override void OnPointerExit( PointerEventData eventData )
        {
            // PointerEventData.fullyExited is false when pointer has exited to enter a child object.
            // This lets me check whether or not the cursor is over any of the descendants, regardless of their position.
            // This also will only be called after the pointer enters the menu and then leaves.
            if( eventData.fullyExited )
            {
                Destroy( this.gameObject );
            }
            base.OnPointerExit( eventData );
        }
    }
}