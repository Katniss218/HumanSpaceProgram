using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UIVesselHUD : MonoBehaviour
    {
        public Vessel Vessel { get; private set; }

        void OnClick()
        {
            ActiveObjectManager.ActiveObject = Vessel.gameObject;
        }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplayCameraController.MainCamera, Vessel.transform.position );
        }

        public static UIVesselHUD Create( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background, Vessel vessel )
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            UIButton button = parent.AddButton( layoutInfo, background, null );

            UIVesselHUD uiHUD = button.gameObject.AddComponent<UIVesselHUD>();
            button.onClick = uiHUD.OnClick;
            uiHUD.Vessel = vessel;
            return uiHUD;
        }
    }
}