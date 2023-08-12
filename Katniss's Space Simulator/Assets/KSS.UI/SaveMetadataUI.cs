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
    public class SaveMetadataUI : MonoBehaviour
    {
        public SaveMetadata Save { get; private set; }

        public static SaveMetadataUI Create( IUIElementContainer parent, UILayoutInfo layout, SaveMetadata save )
        {
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            SaveMetadataUI component = panel.gameObject.AddComponent<SaveMetadataUI>();

            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0, 0.5f ), save.Name );
            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0.5f, 0f ), save.Description );

            return component;
        }
    }
}