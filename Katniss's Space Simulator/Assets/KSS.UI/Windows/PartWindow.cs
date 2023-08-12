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
using UnityPlus.AssetManagement;

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
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 300f, 300f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ )
                .WithRelationHightlight( out WindowRelationHighlight relationHighlight );

            window.AddText( UILayoutInfo.FillHorizontal(0, 0, 1f, 0, 30 ), part.DisplayName )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIScrollView scrollView = window.AddScrollView( UILayoutInfo.Fill( 2, 2, 75, 15 ), new Vector2( 0, 200 ), false, true );

            PartWindow partWindow = window.gameObject.AddComponent<PartWindow>();
            partWindow._list = scrollView.contents.rectTransform;
            partWindow._relationHighlighter = relationHighlight;
            partWindow.SetPart( part );

            return partWindow;
        }
    }
}