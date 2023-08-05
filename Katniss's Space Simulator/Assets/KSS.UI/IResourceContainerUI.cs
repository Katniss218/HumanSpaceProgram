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

        [SerializeField]
        ValueBar _bar;

        void Update()
        {
            _bar.ClearSegments();

            for( int i = 0; i < this.ReferenceObject.Contents.SubstanceCount; i++ )
            {
                var sbs = this.ReferenceObject.Contents[i];
                float perc = (sbs.MassAmount / sbs.Data.Density) / ReferenceObject.MaxVolume;

                var seg = _bar.AddSegment( perc );
                seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/ui_bar" );
                seg.Color = sbs.Data.UIColor;
            }
        }

        public static IResourceContainerUI Create( RectTransform parent, IResourceContainer referenceObj )
        {
            (GameObject root, _) = UIHelper.CreateUI( parent, "Resource Container UI", new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 250, 15 ) ) );

            UIValueBar bar = UIValueBarEx.AddHorizontalValueBar( new UIElement( (RectTransform)root.transform ), new UILayoutInfo( Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.zero ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/ui_bar_background" ) )
                .WithPadding( 5, 5, 1 );

            IResourceContainerUI ui = root.AddComponent<IResourceContainerUI>();
            ui._bar = bar.valueBarComponent;
            ui.ReferenceObject = referenceObj;

            return ui;
        }
    }
}