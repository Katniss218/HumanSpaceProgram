using KSS.Core;
using KSS.Core.DesignScene;
using KSS.Core.DesignScene.Tools;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class PartListEntryUI : MonoBehaviour
    {
        private PartMetadata _part;

        void OnClick()
        {
            // set current tool to pick tool.
            // if vessel exists, add to pick tool
            // otherwise, spawn a new vessel with that part as root.
            PickTool pickTool = DesignSceneToolManager.UseTool<PickTool>();

            GameObject spawnedPart = AssetRegistry.Get<GameObject>( $"part::h/{_part.ID}" );
            if( DesignVesselManager.VesselExists )
            {
                pickTool.HeldPart = spawnedPart.transform;
            }
            else
            {
                spawnedPart.SetLayer( (int)Layer.VESSEL_DESIGN, true );
                spawnedPart.transform.localPosition = Vector3.zero;
                spawnedPart.transform.localRotation = Quaternion.identity;
            }
        }

        public static PartListEntryUI Create( IUIElementContainer parent, UILayoutInfo layout, PartMetadata part )
        {
            UIButton uiButton = parent.AddButton( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_entry_background" ), null )
                .WithText( UILayoutInfo.Fill(), part.Name, out var text );
            text.WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );

            PartListEntryUI partListEntryUI = uiButton.gameObject.AddComponent<PartListEntryUI>();
            partListEntryUI._part = part;

            uiButton.onClick.AddListener( partListEntryUI.OnClick );
            return partListEntryUI;
        }
    }
}