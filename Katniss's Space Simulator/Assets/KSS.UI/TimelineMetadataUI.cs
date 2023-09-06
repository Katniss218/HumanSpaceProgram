using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class TimelineMetadataUI : MonoBehaviour
    {
        public IUIElementContainer saveContainer { get; set; }

        public TimelineMetadata Timeline { get; private set; }

        // raycastee to click on the timeline
        // when clicked, the other panel shows the saves - reference to other panel.

        public static TimelineMetadataUI Create( IUIElementContainer parent, UILayoutInfo layout, TimelineMetadata timeline )
        {
            Debug.Log( "A" );
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );
            panel.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            TimelineMetadataUI component = panel.gameObject.AddComponent<TimelineMetadataUI>();

            UIText t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0, 0.5f ), timeline.Name );
            t.FitToContents = true;
            t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0.5f, 0f ), timeline.Description );
            t.FitToContents = true;

            UILayout.BroadcastLayoutUpdate( panel );

            return component;
        }
    }
}