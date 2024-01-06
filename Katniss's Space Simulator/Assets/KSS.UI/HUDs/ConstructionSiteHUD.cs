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

        public FConstructionSite ConstructionSite { get; private set; }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplayCameraController.MainCamera, ConstructionSite.transform.position );
        }

        public static ConstructionSiteHUD Create( IUIElementContainer parent, FConstructionSite constructionSite )
        {
            if( constructionSite == null )
                throw new ArgumentNullException( nameof( constructionSite ) );

            UIPanel panel = parent.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, Vector2.zero, new Vector2( 50, 50 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_entry_background" ) );
           // UIButton button = parent.AddButton( layoutInfo, background, null );

            ConstructionSiteHUD uiHUD = panel.gameObject.AddComponent<ConstructionSiteHUD>();
            uiHUD.ConstructionSite = constructionSite;
            return uiHUD;
        }
    }
}