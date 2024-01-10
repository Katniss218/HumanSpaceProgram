using KSS.Core;
using KSS.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.HUDs
{
    public class ConstructionSiteHUD : MonoBehaviour
    {
        // construction site hud design doc:

        /*
        
        c-site hud displays the current progress of the construction.

        hover over to get a tooltip with a more detailed description:
        - construction speed (pts/s)
        - points accumulated / total points needed (and percent)
        - if construction is blocked - list the conditions that failed.

        if construction is blocked - display 3 interlocked cogs with a red X.
        if construction is not started - button to start.

        if construction is ongoing - button to pause, if paused - button to resume
        if construction - button to start deconstructing.
        if deconstruction - button to start constructing.

        */

        private UIIcon _statusIcon;

        private UIIcon _progressBar;

        private UIButton _pauseResumeButton;
        private UIButton _reverseButton;

        public FConstructionSite ConstructionSite { get; private set; }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplayCameraController.MainCamera, ConstructionSite.transform.position );
        }

        public static ConstructionSiteHUD Create( IUIElementContainer parent, FConstructionSite constructionSite )
        {
            if( constructionSite == null )
                throw new ArgumentNullException( nameof( constructionSite ) );

            UIPanel panel = parent.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, UILayoutInfo.Middle, UILayoutInfo.BottomLeft, Vector2.zero, new Vector2( 125, 55 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_hud" ) );

            UIIcon progressIcon = panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 21, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_progress_bar" ) );
            UIIcon statusicon = panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 26, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_in_progress" ) );
            UIButton button = panel.AddButton( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 53, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_pause" ), null );
            UIButton revB = panel.AddButton( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 85, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null )
                .WithText( UILayoutInfo.Fill(), "rev.", out _ );

            ConstructionSiteHUD uiHUD = panel.gameObject.AddComponent<ConstructionSiteHUD>();
            uiHUD.ConstructionSite = constructionSite;
            uiHUD._statusIcon = statusicon;
            uiHUD._pauseResumeButton = button;
            uiHUD._reverseButton = revB;
            return uiHUD;
        }
    }
}