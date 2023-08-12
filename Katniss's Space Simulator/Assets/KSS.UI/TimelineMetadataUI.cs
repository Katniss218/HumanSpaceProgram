using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class TimelineMetadataUI : MonoBehaviour
    {
        public TimelineMetadata Timeline { get; private set; }

        public static TimelineMetadataUI Create( IUIElementContainer parent, UILayoutInfo layout, TimelineMetadata timeline )
        {
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            TimelineMetadataUI component = panel.gameObject.AddComponent<TimelineMetadataUI>();

            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0, 0.5f ), timeline.Name );
            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0.5f, 0f ), timeline.Description );

            return component;
        }
    }
}