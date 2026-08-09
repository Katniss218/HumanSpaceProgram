using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using HSP.UI.Windows;
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

            if( !PartRegistry.TryLoad( _part.ID, out GameObject spawnedPart ) )
            {
                UICanvas canvas = DesignSceneM.Instance.GetWindowCanvas();
                Debug.LogError( $"Failed to load part with ID '{_part.ID}' from part list." );
                canvas.AddConfirmWindow( "Error", $"Failed to load part with ID '{_part.ID}'. See log for details.", null );
                return;
            }

            Vessel newVessel = HSP.Vessels.VesselFactory.CreatePartless( HSP.SceneManagement.HSPSceneManager.GetScene( spawnedPart ), HSP.ReferenceFrames.Vector3Dbl.zero, HSP.ReferenceFrames.QuaternionDbl.identity, HSP.ReferenceFrames.Vector3Dbl.zero, HSP.ReferenceFrames.Vector3Dbl.zero );
            spawnedPart.transform.SetParent( newVessel.transform, false );
            spawnedPart.transform.localPosition = Vector3.zero;
            spawnedPart.transform.localRotation = Quaternion.identity;
            
            HSP.Vessels.VesselPart part = spawnedPart.GetComponentInChildren<HSP.Vessels.VesselPart>();
            if (part != null)
            {
                newVessel.SetGraph( HSP.Vessels.VesselAttachmentGraph.Create(part) );
            }

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