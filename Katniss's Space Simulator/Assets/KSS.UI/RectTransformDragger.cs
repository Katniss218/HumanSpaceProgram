using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KSS.UI
{
    /// <summary>
    /// Enables a <see cref="RectTransform"/> to be dragged around by the mouse.
    /// </summary>
    public class RectTransformDragger : EventTrigger
    {
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        [field: SerializeField]
        public PointerEventData.InputButton MouseButton { get; set; } = PointerEventData.InputButton.Left;

        bool _isDragging = false;
        Vector2 _cursorOffset = Vector2.zero;

        void Update()
        {
            if( UITransform == null )
            {
                return;
            }

            if( _isDragging )
            {
                UITransform.position = new Vector2( Input.mousePosition.x, Input.mousePosition.y ) + _cursorOffset;
            }
        }

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

            _cursorOffset = new Vector2( UITransform.position.x, UITransform.position.y ) - new Vector2( Input.mousePosition.x, Input.mousePosition.y );
            _isDragging = true;

            base.OnPointerDown( eventData );
        }

        public override void OnPointerUp( PointerEventData eventData )
        {
            if( UITransform == null )
            {
                return;
            }

            if( eventData.button != MouseButton )
            {
                return;
            }

            _isDragging = false;

            base.OnPointerUp( eventData );
        }
    }
}