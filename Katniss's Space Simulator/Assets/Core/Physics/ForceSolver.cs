using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Physics
{
    /*public class ForceSolver
    {
        public Vector3Dbl Position { get; set; }
        public Vector3Dbl Velocity { get; set; }
        public Vector3Dbl Acceleration { get; private set; }
        public Vector3Dbl ForceAccTimeStep { get; set; }

        /// <summary>
        /// Mass in kgs.
        /// </summary>
        public double Mass { get; set; }



        // THIS CLASS WILL CHANGE A LOT IN THE FUTURE.

        // Supporting collisions between things/landing on surfaces will require some degree of interoperatibility with the Unity rigidbody.
        // - something like back-feeding the data from the collision response to update it maybe? idk yet.
        // - or using the rigidbody directly when necessary (depending on a flag that says close to surface or other vessels).

        // this also needs to support floaring origin/krakensbane equivalent (resetting in-game velocity/position) to keep it running accurately.

        public void AddForce( Vector3 forceNewtons )
        {
            this.ForceAccTimeStep += forceNewtons;
        }

        public void Advance( double dtSeconds )
        {
            // F = m*a
            // a = F/m
            Acceleration = ForceAccTimeStep / Mass;

            // simple euler for now. But consider RK4 or something symplectic for more accuracy for certain applications like orbital mechanics.
            Velocity += Acceleration;

            Position += Velocity * dtSeconds;

            ForceAccTimeStep = Vector3Dbl.zero;
        }
    }*/
}
