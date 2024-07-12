using HSP.Core;
using HSP.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.HUDs
{
    public class ConstructionSiteHUD : UIPanel
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
        private Image _progressImage;
        private UIButton _pauseResumeButton;
        private UIButton _reverseButton;

        public FConstructionSite ConstructionSite { get; private set; }

        private Sprite GetStatusSprite()
        {
            if( ConstructionSite.BuildSpeed == 0.0f )
                return AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_blocked" );

            if( ConstructionSite.GetCountOfProgressing() == 0 )
                return AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_blocked" );

            switch( ConstructionSite.State )
            {
                case ConstructionState.NotStarted:
                    return AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_waiting" );
                case ConstructionState.Constructing:
                case ConstructionState.Deconstructing:
                    return AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_ongoing" );
                case ConstructionState.PausedConstructing:
                case ConstructionState.PausedDeconstructing:
                    return AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_paused" );
            }
            throw new InvalidOperationException( $"Can't get sprite - unknown state of construction." );
        }

        private void PauseResume()
        {
            if( ConstructionSite.State == ConstructionState.NotStarted )
            {
                ConstructionSite.BuildSpeed = 90f; // TODO - remove this once proper build speed (due to nearby cranes) is implemented.
                ConstructionSite.StartConstructing();
                _pauseResumeButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_pause" );
                return;
            }

            if( ConstructionSite.State.IsPaused() )
            {
                ConstructionSite.Unpause();
                _pauseResumeButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_pause" );
            }
            else
            {
                ConstructionSite.Pause();
                _pauseResumeButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_resume" );
            }
        }

        private void Reverse()
        {
            if( ConstructionSite.State.IsConstruction() )
            {
                ConstructionSite.StartDeconstructing();
                _reverseButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_redo" );
            }
            else if( ConstructionSite.State.IsDeconstruction() )
            {
                ConstructionSite.StartConstructing();
                _reverseButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_undo" );
            }
        }

        void LateUpdate()
        {
            (float current, float total) = ConstructionSite.GetBuildPoints();
            float percent = current / total;
            // needs a simple fill image UI component
            _progressImage.fillAmount = percent;
            _statusIcon.Sprite = GetStatusSprite();

            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplaySceneCameraSystem.MainCamera, ConstructionSite.transform.position );
        }

        protected internal static T Create<T>( IUIElementContainer parent, FConstructionSite constructionSite ) where T : ConstructionSiteHUD
        {
            if( constructionSite == null )
                throw new ArgumentNullException( nameof( constructionSite ) );

            T uiHUD = UIPanel.Create<T>( parent, new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.zero, new Vector2( 125, 55 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_hud" ) );

            UIIcon statucIcon = uiHUD.AddIcon( new UILayoutInfo( UIAnchor.TopLeft, (26, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_in_progress" ) );
            UIIcon progressIcon = uiHUD.AddIcon( new UILayoutInfo( UIAnchor.TopLeft, (21, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_progress_bar" ) );

            UIButton pauseResumeButton = uiHUD.AddButton( new UILayoutInfo( UIAnchor.TopLeft, (53, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_resume" ), uiHUD.PauseResume );

            UIButton reverseButton = uiHUD.AddButton( new UILayoutInfo( UIAnchor.TopLeft, (85, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_undo" ), uiHUD.Reverse )
                .WithText( new UILayoutInfo( UIFill.Fill() ), "rev.", out _ );

            uiHUD.ConstructionSite = constructionSite;
            uiHUD._statusIcon = statucIcon;
            uiHUD._pauseResumeButton = pauseResumeButton;
            uiHUD._reverseButton = reverseButton;
            uiHUD._progressBar = progressIcon;
            uiHUD._progressImage = progressIcon.GetComponent<Image>();
            uiHUD._progressImage.type = Image.Type.Filled;
            uiHUD._progressImage.fillMethod = Image.FillMethod.Radial360;
            uiHUD._progressImage.fillOrigin = 2;
            return uiHUD;
        }
    }

    public static class ConstructionSiteHUD_Ex
    {
        public static ConstructionSiteHUD AddConstructionSiteHUD( this IUIElementContainer parent, FConstructionSite vessel )
        {
            return ConstructionSiteHUD.Create<ConstructionSiteHUD>( parent, vessel );
        }
    }
}