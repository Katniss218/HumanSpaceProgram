using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityPlus.UILib
{
    public class RectTransformFocuser : EventTrigger
    {
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        [field: SerializeField]
        public PointerEventData.InputButton MouseButton { get; set; } = PointerEventData.InputButton.Left;

        public override void OnPointerDown( PointerEventData eventData )
        {
            if( UITransform == null )
            {
                return;
            }

            if( eventData.button != MouseButton )
            {
                return;
            }

            this.UITransform.SetSiblingIndex( int.MaxValue );

            base.OnPointerDown( eventData );
        }
    }
}