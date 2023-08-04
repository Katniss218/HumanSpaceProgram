using KSS.Core.ResourceFlowSystem;
using UILib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UILib.Factories;
using KSS.Core;

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
            KSSUIStyle style = (KSSUIStyle)UIStyleManager.Instance.Style;

            for( int i = 0; i < this.ReferenceObject.Contents.SubstanceCount; i++ )
            {
                var sbs = this.ReferenceObject.Contents[i];
                float perc = (sbs.MassAmount / sbs.Data.Density) / ReferenceObject.MaxVolume;

                var seg = _bar.AddSegment( perc );
                seg.Sprite = style.Bar;
                seg.Color = sbs.Data.UIColor;
            }
        }

        public static IResourceContainerUI Create( RectTransform parent, IResourceContainer referenceObj )
        {
            KSSUIStyle style = (KSSUIStyle)UIStyleManager.Instance.Style;

            GameObject root = UIHelper.UI( parent, "resource container UI", Vector2.zero, Vector2.zero, new Vector2( 250, 15 ) );

            (_, ValueBar bar) = UIValueBarEx.CreateEmptyHorizontal( (RectTransform)root.transform, "bar", new UILayoutInfo( Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.zero ), style );

            IResourceContainerUI ui = root.AddComponent<IResourceContainerUI>();
            ui._bar = bar;
            ui.ReferenceObject = referenceObj;

            return ui;
        }
    }
}