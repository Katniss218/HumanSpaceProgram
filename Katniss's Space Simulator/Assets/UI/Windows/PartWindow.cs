using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
using KatnisssSpaceSimulator.UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KatnisssSpaceSimulator.UI.Windows
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

#warning TODO - Find a better way to bind components to their UI elements.
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
            GameObject rootGO = UIHelper.UI( FindObjectOfType<Canvas>().transform, "part window", new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) );

            Image pwBackgroundImage = rootGO.AddComponent<Image>();
            pwBackgroundImage.raycastTarget = true;
            pwBackgroundImage.color = Color.gray;

            GameObject exitButtonGO = UIHelper.UI( rootGO.transform, "X", Vector2.one, Vector2.zero, new Vector2( 30, 30 ) );
            Image exitImage = exitButtonGO.AddComponent<Image>();
            exitImage.raycastTarget = true;
            exitImage.color = Color.red;
            Button exitBtn = exitButtonGO.AddComponent<Button>();
            WindowExit ex = exitButtonGO.AddComponent<WindowExit>();
            ex.ExitButton = exitBtn;
            ex.UITransform = (RectTransform)rootGO.transform;

            GameObject windowContentsGO = UIHelper.UI( rootGO.transform, "contents", Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), new Vector2( 0, -15 ), new Vector2( 0, -30 ) );

            WindowRelationHighlight relationHighlight = rootGO.AddComponent<WindowRelationHighlight>();
            relationHighlight.UITransform = (RectTransform)rootGO.transform;

            GameObject scrollRectGO = UIHelper.AddScrollRect( windowContentsGO, 100 - 30, false, true );

            WindowDrag windowDrag = rootGO.AddComponent<WindowDrag>();
            windowDrag.UITransform = (RectTransform)rootGO.transform;

            PartWindow partWindow = rootGO.AddComponent<PartWindow>();
            partWindow._list = (RectTransform)scrollRectGO.transform;
            partWindow._relationHighlighter = relationHighlight;
            partWindow.SetPart( part );

            return partWindow;
        }
    }
}