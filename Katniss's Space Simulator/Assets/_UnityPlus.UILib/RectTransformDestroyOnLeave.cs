using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class RectTransformDestroyOnLeave : EventTrigger
    {
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
