using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents the orientation of a point in orbit. <br/>
    /// Use this to get orbital directions (prograde/retrograde/etc).
    /// </summary>
    public struct OrbitalFrame
    {
        private Quaternion _orientation;

        /// <summary>
        /// The direction along the velocity vector.
        /// </summary>
        public Vector3 GetPrograde()
        {
            return _orientation.GetForwardAxis();
        }

        /// <summary>
        /// The direction opposite of the velocity vector.
        /// </summary>
        public Vector3 GetRetrograde()
        {
            return _orientation.GetBackAxis();
        }

        /// <summary>
        /// The direction along the orbit's normal vector (right hand rule for angular momentum).
        /// </summary>
        public Vector3 GetNormal()
        {
            return _orientation.GetUpAxis();
        }

        /// <summary>
        /// The direction opposite the orbit's normal vector (right hand rule for angular momentum).
        /// </summary>
        public Vector3 GetAntinormal()
        {
            return _orientation.GetDownAxis();
        }

        /// <summary>
        /// The direction "towards" the attracting body.
        /// </summary>
        public Vector3 GetAntiradial() // antiradial = radial "out"
        {
            return _orientation.GetRightAxis();
        }

        /// <summary>
        /// The direction "away" from the attracting body.
        /// </summary>
        public Vector3 GetRadial() // radial = radial "in"
        {
            return _orientation.GetLeftAxis();
        }

        public static OrbitalFrame FromNBody( Vector3Dbl velocity, Vector3Dbl gravity )
        {
            // Prograde -> towards velocity.
            // Antiradial -> "towards" gravity, but projected onto a plane whose normal is velocity, such that it's orthogonal to Prograde.

            return new OrbitalFrame()
            {
                _orientation = Quaternion.LookRotation( velocity.NormalizeToVector3(), Vector3Dbl.Cross( gravity, velocity ).NormalizeToVector3() )
            };
        }

        public static OrbitalFrame FromKeplerian( Orbit orbit, double ut )
        {
            // For keplerian, you don't get the directions directly, but first compute the orientation and then get the directions from that.

            throw new NotImplementedException();
        }
    }
}