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
        public Transform ReferencePart { get; private set; }

        IUIElementContainer _list;
        WindowRelationHighlight _relationHighlighter;

        public void SetPart( Transform referencePart )
        {
            this.ReferencePart = referencePart;
            this._relationHighlighter.ReferenceTransform = referencePart;
            ReDraw();
        }

        private void ReDraw()
        {
            foreach( UIElement go in _list.Children )
            {
                go.Destroy();
            }

            Component[] components = ReferencePart.GetComponents<Component>();
#warning TODO - if reference part has reference redirection, use the list of parts specified in there to draw components.

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
            if( ReferencePart == null )
            {
                Destroy( this.gameObject );
            }
        }

        public static PartWindow Create( Transform referencePart )
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 300f, 300f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ )
                .WithRelationHightlight( out WindowRelationHighlight relationHighlight );

#warning TODO - proper display names. (display name could be searched towards the root)
            window.AddText( UILayoutInfo.FillHorizontal(0, 0, 1f, 0, 30 ), referencePart.gameObject.name )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIScrollView scrollView = window.AddVerticalScrollView( UILayoutInfo.Fill( 2, 2, 75, 15 ), 200 );

            PartWindow partWindow = window.gameObject.AddComponent<PartWindow>();
            partWindow._list = scrollView;
            partWindow._relationHighlighter = relationHighlight;
            partWindow.SetPart( referencePart );

            return partWindow;
        }
    }
}