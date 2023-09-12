using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class SaveMetadataUI : EventTrigger
    {
        public SaveMetadata Save { get; private set; }

        public Action<SaveMetadataUI> onClick;

        public override void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke(this);
            }

            base.OnPointerClick( eventData );
        }

        public static SaveMetadataUI Create( IUIElementContainer parent, UILayoutInfo layout, SaveMetadata save, Action<SaveMetadataUI> onClick )
        {
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            SaveMetadataUI component = panel.gameObject.AddComponent<SaveMetadataUI>();
            component.Save = save;
            component.onClick = onClick;

            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0, 0.5f ), save.Name );
            panel.AddText( UILayoutInfo.FillPercent( 0, 0, 0.5f, 0f ), save.Description );

            UILayout.BroadcastLayoutUpdate( panel );

            return component;
        }
    }
}