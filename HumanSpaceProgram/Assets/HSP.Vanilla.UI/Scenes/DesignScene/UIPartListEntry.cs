using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using HSP.SceneManagement;
using HSP.UI.Windows;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.DesignScene.Tools;
using HSP.Vessels;
using HSP.Vessels.Serialization;
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
            // (always) set current tool to pick tool.
            // if a vessel exists, add new new graph to the pick tool.
            // otherwise, spawn a new vessel with that graph.
            PickTool pickTool = DesignSceneToolManager.UseTool<PickTool>();

            if( !VesselSerializationUtils.TryLoad( _part.Filepath, out IPartGraph partGraph ) )
            {
                UICanvas canvas = DesignSceneM.Instance.GetWindowCanvas();
                Debug.LogError( $"Failed to load part with ID '{_part.ID}' from part list." );
                canvas.AddConfirmWindow( "Error", $"Failed to load part with ID '{_part.ID}'. See log for details.", null );
                return;
            }

            Vessel newVessel = VesselFactory.CreatePartless<Vessel>( DesignSceneM.Instance, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero );
            
            if( DesignVesselManager.DesignObject == null )
            {
                newVessel.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
                DesignVesselManager.SetDesignObject( newVessel );
            }
            else
            {
                pickTool.SetHeldPart( newVessel, Vector3.zero, Quaternion.identity );
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