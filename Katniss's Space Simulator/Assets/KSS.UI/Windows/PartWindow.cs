using KSS.Cameras;
using KSS.Core;
using KSS.Core.ResourceFlowSystem;
using UnityPlus.UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    /// <summary>
    /// A pop-up window for a <see cref="Core.Part"/>.
    /// </summary>
    public class PartWindow : EventTrigger
    {
        /// <summary>
        /// The part that is currently referenced by this part window.
        /// </summary>
        [field: SerializeField]
        public Part Part { get; private set; }

        RectTransform _list;
        WindowRelationHighlight _relationHighlighter;

        public void SetPart( Part part )
        {
            this.Part = part;
            _relationHighlighter.ReferenceTransform = part.transform;
            ReDraw();
        }

        private void ReDraw()
        {
            foreach( GameObject go in _list )
            {
                Destroy( go );
            }

            Component[] components = Part.GetComponents<Component>();

#warning TODO - Find a better way to overridably bind components to their UI elements.
            // one kinda ugly way would be to put the type in the path.
            // one slightly better way would be to put the object in the argument, but that will call every listener, most of which won't care about this specific object.
            // also, make the event type-safe, event creation specifies type, later assignments must use a delegate that equals the created type.

            foreach( var comp in components )
            {
                if( comp is IResourceContainer r )
                {
                    IResourceContainerUI.Create( _list, r );
                }
            }
        }

        void LateUpdate()
        {
            if( Part == null )
            {
                Destroy( this.gameObject );
            }
        }

        public static PartWindow Create( Part part )
        {
            (GameObject rootGO, RectTransform rootRT) = UIHelper.CreateUI( (UIElement)CanvasManager.Get( CanvasName.WINDOWS ).transform, "part window", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ) );

            WindowRelationHighlight relationHighlight = rootGO.AddComponent<WindowRelationHighlight>();
            relationHighlight.UITransform = (RectTransform)rootGO.transform;

            RectTransformDragger windowDrag = rootGO.AddComponent<RectTransformDragger>();
            windowDrag.UITransform = (RectTransform)rootGO.transform;

            Image pwBackgroundImage = rootGO.AddComponent<Image>();
            pwBackgroundImage.raycastTarget = true;
            pwBackgroundImage.color = Color.gray;

            (GameObject exitButtonGO, RectTransform exitRT) = UIHelper.CreateUI( rootRT, "X", new UILayoutInfo( Vector2.one, Vector2.zero, new Vector2( 30, 30 )) );
            Image exitImage = exitButtonGO.AddComponent<Image>();
            exitImage.raycastTarget = true;
            exitImage.color = Color.red;
            Button exitBtn = exitButtonGO.AddComponent<Button>();

            RectTransformCloser ex = exitButtonGO.AddComponent<RectTransformCloser>();
            ex.ExitButton = exitBtn;
            ex.UITransform = (RectTransform)rootGO.transform;

            UIScrollView scrollView = ((UIElement)rootRT).AddScrollView( new UILayoutInfo( Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), new Vector2( 0, -15 ), new Vector2( 0, -30 )), new Vector2(0, 100 - 30), false, true );

            PartWindow partWindow = rootGO.AddComponent<PartWindow>();
            partWindow._list = scrollView.contents.transform;
            partWindow._relationHighlighter = relationHighlight;
            partWindow.SetPart( part );

            return partWindow;
        }
    }
}