using KSS.Core.ResourceFlowSystem;
using UnityPlus.UILib;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib.UIElements;
using KSS.Core.Serialization;
using System;

namespace KSS.UI
{
    public class UIIResourceContainer : UIPanel
    {
        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public IResourceContainer ReferenceObject { get; set; }

        UIValueBar _bar;

        void Update()
        {
#warning TODO - Inefficient as fuck.
            _bar.ClearSegments();

            for( int i = 0; i < this.ReferenceObject.Contents.SubstanceCount; i++ )
            {
                var sbs = this.ReferenceObject.Contents[i];
                float perc = (sbs.MassAmount / sbs.Substance.Density) / ReferenceObject.MaxVolume;

                var seg = _bar.AddSegment( perc );
                seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
                seg.Color = sbs.Substance.UIColor;
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, IResourceContainer referenceObj ) where T : UIIResourceContainer
        {
            T uiIResourceContainer = UIPanel.Create<T>( parent, new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (250, 15) ), null );

            UIValueBar uiValueBar = uiIResourceContainer.AddHorizontalValueBar( new UILayoutInfo( UIFill.Fill() ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            uiIResourceContainer.ReferenceObject = referenceObj;
            uiIResourceContainer._bar = uiValueBar;

            return uiIResourceContainer;
        }
    }

    public static class UIIResourceContainer_Ex
    {
        public static UIIResourceContainer AddIResourceContainer( this IUIElementContainer parent, IResourceContainer referenceObj )
        {
            return UIIResourceContainer.Create<UIIResourceContainer>( parent, referenceObj );
        }
    }
}