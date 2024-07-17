using HSP.Core;
using HSP.Core.Components;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using HSP.Core.Serialization;
using HSP.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI
{
    public class UINavball : UIPanel
    {
        private UIIcon _prograde;
        private UIIcon _retrograde;
        private UIIcon _normal;
        private UIIcon _antinormal;
        private UIIcon _antiradial;
        private UIIcon _radial;

        private UIIcon _maneuver; // maybe instead of that, add a list of custom icons?

        public float NavballPixelRadius { get; set; } = 95f;

        public Vector3? ManeuverAirfDirection { get; set; } = Vector3.one;

        public void SetDirectionIcons( UIIcon prograde, UIIcon retrograde, UIIcon normal, UIIcon antinormal, UIIcon antiradial, UIIcon radial, UIIcon maneuver )
        {
            _prograde = prograde;
            _retrograde = retrograde;
            _normal = normal;
            _antinormal = antinormal;
            _antiradial = antiradial;
            _radial = radial;
            _maneuver = maneuver;
        }

        static Vector3 CulledByDepth( Vector3 pos )
        {
            return pos.z >= 0 ? pos : new Vector2( 9999, 9999 ); ;
        }

        void LateUpdate()
        {
            if( ActiveObjectManager.ActiveObject != null )
            {
                IVessel activeVessel = ActiveObjectManager.ActiveObject.transform.GetVessel();

                Quaternion airfRotation = (Quaternion)FControlFrame.GetAIRFRotation( FControlFrame.VesselControlFrame, activeVessel );
                Matrix4x4 airfToLocalMatrix = Matrix4x4.Rotate( airfRotation ).inverse;

                Vector3Dbl airfVelocity = SceneReferenceFrameManager.SceneReferenceFrame.TransformDirection( activeVessel.PhysicsObject.Velocity );
                if( airfVelocity.magnitude > 0.25f )
                {
                    OrbitalFrame orbitalFrame = OrbitalFrame.FromNBody( airfVelocity, GravityUtils.GetNBodyGravityAcceleration( activeVessel.RootObjTransform.AIRFPosition ) );

                    Vector3 localPrograde = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetPrograde() ) * NavballPixelRadius;
                    Vector3 localRetrograde = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetRetrograde() ) * NavballPixelRadius;
                    Vector3 localNormal = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetNormal() ) * NavballPixelRadius;
                    Vector3 localAntinormal = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetAntinormal() ) * NavballPixelRadius;
                    Vector3 localAntiradial = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetAntiradial() ) * NavballPixelRadius;
                    Vector3 localRadial = airfToLocalMatrix.MultiplyVector( orbitalFrame.GetRadial() ) * NavballPixelRadius;

                    _prograde.rectTransform.anchoredPosition = CulledByDepth( localPrograde );
                    _retrograde.rectTransform.anchoredPosition = CulledByDepth( localRetrograde );
                    _normal.rectTransform.anchoredPosition = CulledByDepth( localNormal );
                    _antinormal.rectTransform.anchoredPosition = CulledByDepth( localAntinormal );
                    _antiradial.rectTransform.anchoredPosition = CulledByDepth( localAntiradial );
                    _radial.rectTransform.anchoredPosition = CulledByDepth( localRadial );
                }
                else
                {
                    _prograde.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                    _retrograde.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                    _normal.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                    _antinormal.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                    _antiradial.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                    _radial.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                }

                if( ManeuverAirfDirection.HasValue )
                {
                    Vector3 localManeuver = airfToLocalMatrix.MultiplyVector( ManeuverAirfDirection.Value.normalized ) * NavballPixelRadius;
                    _maneuver.rectTransform.anchoredPosition = CulledByDepth( localManeuver );
                }
                else
                {
                    _maneuver.rectTransform.anchoredPosition = new Vector2( 9999, 9999 );
                }
            }
        }

        public static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UINavball
        {
            T navball = UIPanel.Create<T>( parent, layout, null );

            UIMask mask = navball.AddMask( new UILayoutInfo( UIAnchor.Center, (0, 0), (190, 190) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/std0/ui_navball" ) );

            (GameObject rawGameObject, RectTransform rawTransform) = UIElement.CreateUIGameObject( mask.rectTransform, "raw", new UILayoutInfo( UIFill.Fill() ) );
            RawImage rawImage = rawGameObject.AddComponent<RawImage>();
            rawImage.texture = NavballRenderTextureManager.AttitudeIndicatorRT;

            UIIcon attitudeIndicator = navball.AddIcon( new UILayoutInfo( UIFill.Fill() ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/attitude_indicator" ) );


            UIIcon prograde = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_prograde" ) );
            UIIcon retrograde = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_retrograde" ) );
            UIIcon normal = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_normal" ) );
            UIIcon antinormal = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_antinormal" ) );
            UIIcon antiradial = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_radial_out" ) );
            UIIcon radial = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_radial_in" ) );
            UIIcon maneuver = mask.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (34, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_maneuver" ) );

            navball.SetDirectionIcons( prograde, retrograde, normal, antinormal, antiradial, radial, maneuver );

            UIIcon horizon = navball.AddIcon( new UILayoutInfo( UIAnchor.Center, (0, 0), (90, 32) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/nodes/m_horizon" ) );


            UIPanel velocityIndicator = navball.AddPanel( new UILayoutInfo( UIAnchor.Top, (0, 15), (167.5f, 40) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/velocity_indicator" ) );

            velocityIndicator.AddButton( new UILayoutInfo( UIAnchor.Left, (2, 0), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ), null );
            velocityIndicator.AddButton( new UILayoutInfo( UIAnchor.Right, (-2, 0), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_down_gold" ), null );

            velocityIndicator.AddTextReadout_Velocity( new UILayoutInfo( UIFill.Fill( 31.5f, 31.5f, 0, 20 ) ) )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            velocityIndicator.AddTextReadout_Altitude( new UILayoutInfo( UIFill.Fill( 31.5f, 31.5f, 20, 0 ) ) )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            return navball;
        }
    }

    public static class UINavball_Ex
    {
        public static UINavball AddNavball( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UINavball.Create<UINavball>( parent, layout );
        }
    }
}