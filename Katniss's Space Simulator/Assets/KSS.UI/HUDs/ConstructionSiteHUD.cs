﻿using KSS.Core;
using KSS.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        void LateUpdate()
        {
            (float current, float total) = ConstructionSite.GetBuildPoints();
            float percent = current / total;
            // needs a simple fill image UI component
            _progressImage.fillAmount = percent;
            _statusIcon.Sprite = GetStatusSprite();

            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplayCameraController.MainCamera, ConstructionSite.transform.position );
        }

        public static ConstructionSiteHUD Create( IUIElementContainer parent, FConstructionSite constructionSite )
        {
            if( constructionSite == null )
                throw new ArgumentNullException( nameof( constructionSite ) );

            UIPanel panel = parent.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, UILayoutInfo.Middle, UILayoutInfo.BottomLeft, Vector2.zero, new Vector2( 125, 55 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_hud" ) );

            UIIcon statucIcon = panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 26, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_status_in_progress" ) );
            UIIcon progressIcon = panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 21, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/csite_progress_bar" ) );
            UIButton pauseResumeButton = panel.AddButton( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 53, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30_pause" ), null );
            UIButton reverseButton = panel.AddButton( new UILayoutInfo( UILayoutInfo.TopLeft, new Vector2( 85, 0 ), new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null )
                .WithText( UILayoutInfo.Fill(), "rev.", out _ );

            ConstructionSiteHUD uiHUD = panel.gameObject.AddComponent<ConstructionSiteHUD>();
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
}