using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KatnisssSpaceSimulator.Assets.UI.Windows
{
    public class WindowDrag : EventTrigger
    {
        [SerializeField] public RectTransform UITransform;

        bool isDragging = false;
        Vector2 cursorOffset = Vector2.zero;

        void Update()
        {
            if( isDragging )
            {
                UITransform.position = new Vector2( Input.mousePosition.x, Input.mousePosition.y ) + cursorOffset;
            }
        }

        public override void OnPointerDown( PointerEventData eventData )
        {
            if( eventData.button != PointerEventData.InputButton.Left )
            {
                return;
            }

            cursorOffset = new Vector2( UITransform.position.x, UITransform.position.y ) - new Vector2( Input.mousePosition.x, Input.mousePosition.y );
            isDragging = true;

            base.OnPointerDown( eventData );
        }

        public override void OnPointerUp( PointerEventData eventData )
        {
            if( eventData.button != PointerEventData.InputButton.Left )
            {
                return;
            }

            isDragging = false;

            base.OnPointerUp( eventData );
        }
    }
}