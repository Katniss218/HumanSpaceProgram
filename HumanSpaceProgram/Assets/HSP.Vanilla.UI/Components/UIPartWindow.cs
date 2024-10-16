﻿using HSP.Content.Vessels.Serialization;
using HSP.UI;
using HSP.Vessels.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    /// <summary>
    /// Invoked once for every component, every time the part window is redrawn. Use this event to create FComponent UI elements.
    /// </summary>
    public static class HSPEvent_ON_PART_WINDOW_REDRAW
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".part_window_redraw";
    }

    /// <summary>
    /// A pop-up window containing the information about some part of a vessel.
    /// </summary>
    public class UIPartWindow : UIWindow
    {
        static List<UIPartWindow> _activePartWindows = new List<UIPartWindow>();

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

            IEnumerable<Component> components = ReferencePart.GetComponents<Component>().OrderBy( c => c.GetType().Name );

            foreach( var comp in components )
            {
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_PART_WINDOW_REDRAW.ID, (this._list, comp) );
            }
        }

        void LateUpdate()
        {
            if( ReferencePart == null )
            {
                Destroy( this.gameObject );
            }
        }

        /// <summary>
        /// Checks if there is a part window being displayed for the specified transform.
        /// </summary>
        public static bool ExistsFor( Transform referencePart )
        {
            // Remove destroyed windows, and windows referencing destroyed parts (if any leaked out) from the list.
            _activePartWindows = _activePartWindows.Where( pw => pw != null && pw.ReferencePart != null ).ToList();

            if( referencePart == null )
            {
                throw new ArgumentNullException( nameof( referencePart ), $"Part windows don't exist for nonexistant parts." );
            }

            foreach( var partWindow in _activePartWindows )
            {
                if( partWindow.ReferencePart == referencePart )
                {
                    return true;
                }
            }
            return false;
        }

        /// <param name="referencePart">The transform that will serve as the root for the part window.</param>
        public static T Create<T>( UICanvas parent, UILayoutInfo layout, Transform referencePart ) where T : UIPartWindow
        {
            // Note that this method shouldn't handle any redirecting,
            // so if you invoke it with the transform of a collider that doesn't have any functionalities attached, it will not show anything.
            if( referencePart == null )
            {
                throw new ArgumentNullException( nameof( referencePart ), $"Can't create a part window for a nonexistent part." );
            }
            PartMetadata part = FPart.GetPart( referencePart );
            if( part == null )
            {
                throw new ArgumentNullException( nameof( referencePart ), $"Can't create a part window for an object that can't be mapped to a {nameof( PartMetadata )}." );
            }

            T partWindow = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ )
                .WithRelationHightlight( out WindowRelationHighlight relationHighlight );

            // masses/etc can be summed up from components in children of reference part.

            partWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), part.Name )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            UIScrollView scrollView = partWindow.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill( 2, 2, 75, 15 ) ), 200 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical" ), out _ );

            scrollView.LayoutDriver = new VerticalLayoutDriver()
            {
                Dir = VerticalLayoutDriver.Direction.TopToBottom,
                Spacing = 2f,
                FitToSize = true
            };

            partWindow._list = scrollView;
            partWindow._relationHighlighter = relationHighlight;
            partWindow.SetPart( referencePart );
            _activePartWindows.Add( partWindow );

            return partWindow;
        }
    }

    public static class UIPartWindow_Ex
    {
        public static UIPartWindow AddPartWindow( this UICanvas parent, UILayoutInfo layout, Transform referencePart )
        {
            return UIPartWindow.Create<UIPartWindow>( parent, layout, referencePart );
        }
    }
}