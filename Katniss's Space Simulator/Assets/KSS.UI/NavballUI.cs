using KSS.Core;
using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using KSS.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class NavballUI : MonoBehaviour
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
                Vessel activeVessel = ActiveObjectManager.ActiveObject.transform.GetVessel();

                Matrix4x4 airfToLocalMatrix = Matrix4x4.Rotate( (Quaternion)activeVessel.AIRFRotation ).inverse;

                Vector3Dbl airfVelocity = SceneReferenceFrameManager.SceneReferenceFrame.TransformDirection( activeVessel.PhysicsObject.Velocity );
                if( airfVelocity.magnitude > 0.25f )
                {
                    OrbitalFrame airfOrientation = OrbitalFrame.FromNBody( airfVelocity, GravityUtils.GetNBodyGravityAcceleration( activeVessel.AIRFPosition ) );

                    Vector3 localPrograde = airfToLocalMatrix.MultiplyVector( airfOrientation.GetPrograde() ) * NavballPixelRadius;
                    Vector3 localRetrograde = airfToLocalMatrix.MultiplyVector( airfOrientation.GetRetrograde() ) * NavballPixelRadius;
                    Vector3 localNormal = airfToLocalMatrix.MultiplyVector( airfOrientation.GetNormal() ) * NavballPixelRadius;
                    Vector3 localAntinormal = airfToLocalMatrix.MultiplyVector( airfOrientation.GetAntinormal() ) * NavballPixelRadius;
                    Vector3 localAntiradial = airfToLocalMatrix.MultiplyVector( airfOrientation.GetAntiradial() ) * NavballPixelRadius;
                    Vector3 localRadial = airfToLocalMatrix.MultiplyVector( airfOrientation.GetRadial() ) * NavballPixelRadius;

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
    }
}