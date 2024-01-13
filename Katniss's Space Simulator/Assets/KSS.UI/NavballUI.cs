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

        private UIIcon _maneuver; // maybe instead of that, add custom icons?

        public void SetDirectionIcons( UIIcon prograde, UIIcon retrograde, UIIcon normal, UIIcon antinormal, UIIcon antiradial, UIIcon radial )
        {
            _prograde = prograde;
            _retrograde = retrograde;
            _normal = normal;
            _antinormal = antinormal;
            _antiradial = antiradial;
            _radial = radial;
        }

        void LateUpdate()
        {
            if( ActiveObjectManager.ActiveObject != null )
            {
                // these directions can be mapped onto the navball plane by simply transforming them using the vessel space rotation matrix.
                // - get world-to-local rotation matrix from airf orientation
                // - get directions
                // - multiply directions by the matrix
                // - depth value indicates whether or not to fade and/or show the icon (behind the ball)

                Vessel activeVessel = ActiveObjectManager.ActiveObject.transform.GetVessel();
                Quaternion orbitalOrientation = NavballUtils.GetOrientation( SceneReferenceFrameManager.SceneReferenceFrame.TransformDirection( activeVessel.PhysicsObject.Velocity ), GravityUtils.GetNBodyGravityAcceleration( activeVessel.AIRFPosition ) );

                Matrix4x4 rotation = Matrix4x4.Rotate( (Quaternion)activeVessel.AIRFRotation ).inverse;

                const float NAVBALL_SIZE = 95f;

                Vector3 localPrograde = rotation.MultiplyVector( NavballUtils.GetPrograde( orbitalOrientation ) ) * NAVBALL_SIZE;
                Vector3 localRetrograde = rotation.MultiplyVector( NavballUtils.GetRetrograde( orbitalOrientation ) ) * NAVBALL_SIZE;
                Vector3 localNormal = rotation.MultiplyVector( NavballUtils.GetNormal( orbitalOrientation ) ) * NAVBALL_SIZE;
                Vector3 localAntinormal = rotation.MultiplyVector( NavballUtils.GetAntinormal( orbitalOrientation ) ) * NAVBALL_SIZE;
                Vector3 localAntiradial = rotation.MultiplyVector( NavballUtils.GetAntiradial( orbitalOrientation ) ) * NAVBALL_SIZE;
                Vector3 localRadial = rotation.MultiplyVector( NavballUtils.GetRadial( orbitalOrientation ) ) * NAVBALL_SIZE;

                _prograde.rectTransform.anchoredPosition = localPrograde.z >= 0 ? localPrograde : new Vector2( 9999, 9999 );
                _retrograde.rectTransform.anchoredPosition = localRetrograde.z >= 0 ? localRetrograde : new Vector2( 9999, 9999 );
                _normal.rectTransform.anchoredPosition = localNormal.z >= 0 ? localNormal : new Vector2( 9999, 9999 );
                _antinormal.rectTransform.anchoredPosition = localAntinormal.z >= 0 ? localAntinormal : new Vector2( 9999, 9999 );
                _antiradial.rectTransform.anchoredPosition = localAntiradial.z >= 0 ? localAntiradial : new Vector2( 9999, 9999 );
                _radial.rectTransform.anchoredPosition = localRadial.z >= 0 ? localRadial : new Vector2( 9999, 9999 );


            }
        }
    }
}