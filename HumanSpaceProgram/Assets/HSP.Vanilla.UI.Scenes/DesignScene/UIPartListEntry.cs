using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.DesignScene.Tools;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.DesignScene
{
    public class UIPartListEntry : UIButton
    {
        private PartMetadata _part;

        void OnClick()
        {
            // set current tool to pick tool.
            // if vessel exists, add to pick tool
            // otherwise, spawn a new vessel with that part as root.
            PickTool pickTool = DesignSceneToolManager.UseTool<PickTool>();

            GameObject spawnedPart = PartRegistry.Load( _part.ID );
            if( DesignVesselManager.DesignObject == null )
            {
                spawnedPart.transform.localPosition = Vector3.zero;
                spawnedPart.transform.localRotation = Quaternion.identity;
                spawnedPart.SetLayer( (int)Layer.PART_OBJECT, true );
                DesignVesselManager.TryAttachRoot( spawnedPart.transform );
            }
            else
            {
                pickTool.SetHeldPart( spawnedPart.transform, Vector3.zero );
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, PartMetadata part ) where T : UIPartListEntry
        {
            T partListEntryUI = UIButton.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_entry_background" ), null )
                .WithText( new UILayoutInfo( UIFill.Fill() ), part.Name, out var text );

            text.WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
            text.WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );

            partListEntryUI._part = part;
            partListEntryUI.onClick = partListEntryUI.OnClick;

            return partListEntryUI;
        }
    }

    public static class UIPartListEntry_Ex
    {
        public static UIPartListEntry AddPartListEntry( this IUIElementContainer parent, UILayoutInfo layout, PartMetadata part )
        {
            return UIPartListEntry.Create<UIPartListEntry>( parent, layout, part );
        }
    }
}