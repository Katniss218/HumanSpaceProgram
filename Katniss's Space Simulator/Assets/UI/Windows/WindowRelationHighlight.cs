using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KatnisssSpaceSimulator.Assets.UI.Windows
{
    /// <summary>
    /// A script that draws a graphical relationship between a UI element, and a scene object, when the UI element is hovered over.
    /// </summary>
    public class WindowRelationHighlight : MonoBehaviour
    {
        /// <summary>
        /// The UI element
        /// </summary>
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        /// <summary>
        /// The scene object to relate to.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceTransform { get; set; }


        RectTransform _isHighlighting = null;

        void OnPointerEnter()
        {
            GameObject highlighterGO = UIHelper.UI( this.transform, "relation highlight", Vector2.zero, Vector2.zero, new Vector2( 10, 10 ) );
            _isHighlighting = (RectTransform)highlighterGO.transform;

            Image exitImage = highlighterGO.AddComponent<Image>();
            exitImage.raycastTarget = true;
            exitImage.color = Color.green;
        }

        void OnPointerExit()
        {
            Destroy( _isHighlighting.gameObject );
            _isHighlighting = null;
        }

        void LateUpdate()
        {
            if( ReferenceTransform == null )
            {
                return;
            }

            // RectangleContainsScreenPoint will force the relationship to be drawn regardless is some graphics raycaster covers the UI element.
            if( RectTransformUtility.RectangleContainsScreenPoint( UITransform, Input.mousePosition, null ) )
            {
                if( _isHighlighting == null )
                {
                    OnPointerEnter();
                }
            }
            else
            {
                if( _isHighlighting != null )
                {
                    OnPointerExit();
                }
            }

            if( _isHighlighting )
            {
                _isHighlighting.position = CameraController.Instance.MainCamera.WorldToScreenPoint( ReferenceTransform.position );
            }
        }
    }
}