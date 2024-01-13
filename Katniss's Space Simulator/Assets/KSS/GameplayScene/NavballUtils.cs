using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.GameplayScene
{
    public static class NavballUtils
    {
        public static Quaternion GetOrientation( Vector3Dbl velocity, Vector3Dbl gravity )
        {
            // antiradial points "towards" gravity (but is in plane where velocity is its normal)
            // prograde points towards velocity.

            return Quaternion.LookRotation( velocity.NormalizeToVector3(), Vector3Dbl.Cross( gravity, velocity ).NormalizeToVector3() );
        }

        public static Vector3 GetPrograde( Quaternion orientation )
        {
            return orientation.GetForwardAxis();
        }

        public static Vector3 GetRetrograde( Quaternion orientation )
        {
            return orientation.GetBackAxis();
        }

        public static Vector3 GetNormal( Quaternion orientation )
        {
            return orientation.GetUpAxis();
        }

        public static Vector3 GetAntinormal( Quaternion orientation )
        {
            return orientation.GetDownAxis();
        }

        public static Vector3 GetAntiradial( Quaternion orientation ) // antiradial = radial "out"
        {
            return orientation.GetRightAxis();
        }

        public static Vector3 GetRadial( Quaternion orientation ) // radial = radial "in"
        {
            return orientation.GetLeftAxis();
        }
    }
}