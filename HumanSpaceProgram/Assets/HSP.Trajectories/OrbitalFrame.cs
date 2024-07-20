using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents the orientation of a point in orbit. <br/>
    /// Use this to get orbital directions (prograde/retrograde/etc).
    /// </summary>
    public struct OrbitalFrame
    {
        private readonly Quaternion _rotation;

        /// <summary>
        /// The direction along the velocity vector.
        /// </summary>
        public Vector3 GetPrograde()
        {
            return _rotation.GetForwardAxis();
        }

        /// <summary>
        /// The direction opposite of the velocity vector.
        /// </summary>
        public Vector3 GetRetrograde()
        {
            return _rotation.GetBackAxis();
        }

        /// <summary>
        /// The direction along the orbit's normal vector (right hand rule for angular momentum).
        /// </summary>
        public Vector3 GetNormal()
        {
            return _rotation.GetLeftAxis();
        }

        /// <summary>
        /// The direction opposite the orbit's normal vector (right hand rule for angular momentum).
        /// </summary>
        public Vector3 GetAntinormal()
        {
            return _rotation.GetRightAxis();
        }

        /// <summary>
        /// The direction "towards" the attracting body.
        /// </summary>
        public Vector3 GetAntiradial() // antiradial = radial "out"
        {
            return _rotation.GetUpAxis();
        }

        /// <summary>
        /// The direction "away" from the attracting body.
        /// </summary>
        public Vector3 GetRadial() // radial = radial "in"
        {
            return _rotation.GetDownAxis();
        }

        public OrbitalFrame( Quaternion rotation )
        {
            _rotation = rotation;
        }

        public OrbitalFrame( Vector3 forward, Vector3 up )
        {
            _rotation = Quaternion.LookRotation( forward, up );
        }
    }
}