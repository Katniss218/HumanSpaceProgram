using KSS.Core.ResourceFlowSystem;
using UnityPlus.UILib;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class IResourceContainerUI : MonoBehaviour
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

        public static IResourceContainerUI Create( IUIElementContainer parent, IResourceContainer referenceObj )
        {
            UIPanel panel = parent.AddPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (250, 15) ), null );

            UIValueBar bar = panel.AddHorizontalValueBar( new UILayoutInfo( UIFill.Fill() ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            IResourceContainerUI ui = panel.gameObject.AddComponent<IResourceContainerUI>();
            ui._bar = bar;
            ui.ReferenceObject = referenceObj;

            return ui;
        }
    }
}