using HSP.Trajectories;
using System;
using UnityEngine;

namespace HSP.Core
{
    /// <summary>
    /// A trajectory that follows kepler's laws of planetary motion.
    /// </summary>
    public class KeplerianOrbit : ITrajectory
    {
        // Sources:
        // https://github.com/MuMech/MechJeb2/blob/dev/MechJeb2/MechJebLib/Core/Maths.cs#L226
        // https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf
        // https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf

        // Orbits are always in body-centric frame, with the coordinate axes aligned with Absolute Inertial Reference Frame.
        // State vectors too.

        /// <summary>
        /// The value of the gravitational constant, in [m^3/kg/s^2].
        /// </summary>
        public const double G = 6.6743e-11;

        /// <summary>
        /// The semi-major axis of the orbit, in [m].
        /// </summary>
        public double semiMajorAxis;

        /// <summary>
        /// The eccentricity of the orbit, in [0] for circular, [0..1] for elliptical, [1] for parabolic, [1..] for hyperbolic.
        /// </summary>
        public double eccentricity;

        /// <summary>
        /// The inclination of the orbit, in [Rad].
        /// </summary>
        public double inclination;

        /// <summary>
        /// The longitude of the ascending node of the orbit, in [Rad].
        /// </summary>
        public double longitudeOfAscendingNode;

        /// <summary>
        /// The argument of periapsis of the orbit, in [Rad].
        /// </summary>
        public double argumentOfPeriapsis;

        /// <summary>
        /// The mean anomaly of the orbiting object, at <see cref="epoch"/>, in [Rad]. <br />
        /// Mean anomaly is the angle between the periapsis, the central body, and the orbiting body, if the orbit was circular and with the same orbital period.
        /// </summary>
        /// <remarks>
        /// This angle can be set outside the standard range [0..2*PI].
        /// </remarks>
        public double meanAnomaly; // We store the mean anomaly because it is convenient.

        /// <summary>
        /// The reference epoch for the orbit, UT, in [s].
        /// </summary>
        public double epoch;

        /// <summary>
        /// The length of half of the chord line perpendicular to the major axis, and passing through the focus.
        /// </summary>
        public double semiLatusRectum => semiMajorAxis * (1 - (eccentricity * eccentricity));

        public KeplerianOrbit( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, double epoch )
        {
            this.semiMajorAxis = semiMajorAxis;
            this.eccentricity = eccentricity;
            this.inclination = inclination;
            this.longitudeOfAscendingNode = longitudeOfAscendingNode;
            this.argumentOfPeriapsis = argumentOfPeriapsis;
            this.meanAnomaly = meanAnomaly;
            this.epoch = epoch;
        }

        /// <summary>
        /// Returns the orbital mean motion for a circular orbit of the same orbital period. <br />
        /// This can be used to calculate the mean anomaly at a different time.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The mean motion, in [Rad/s].</returns>
        public double GetMeanMotion( double gravParameter )
        {
            return Math.Sqrt( gravParameter / Math.Abs( semiMajorAxis * semiMajorAxis * semiMajorAxis ) );
        }

        /// <summary>
        /// calculates the standard gravitational parameter for a celestial body with the specified mass.
        /// </summary>
        /// <param name="bodyMass">The mass of the celestial body, in [kg].</param>
        /// <returns>The standard gravitational parameter for a body with the given mass, in [m^3/s^2].</returns>
        public double GetGravParameter( double bodyMass )
        {
            return G * bodyMass;
        }

        /// <summary>
        /// Calculates the orbital period of this orbit, around a given body.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The orbital period, in [s].</returns>
        public double GetOrbitalPeriod( double gravParameter )
        {
            return (2 * Math.PI) * Math.Sqrt( (semiMajorAxis * semiMajorAxis * semiMajorAxis) / gravParameter );
        }

        /// <summary>
        /// Calculates the escape velocity at a point a certain distance away from the center of gravity.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <param name="radius">The distance from the center of gravity of the parent body, in [m].</param>
        /// <returns>The escape velocity, in [m/s]</returns>
        public static double GetEscapeVelocity( double gravParameter, double radius )
        {
            return Math.Sqrt( (2 * gravParameter) / radius );
        }

        /// <summary>
        /// Returns the semi-major axis for a circular or elliptical orbit with a given orbital period.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <param name="targetPeriod">The desired period, in [s].</param>
        /// <returns>The semi-major axis, in [m].</returns>
        public double GetSemiMajorAxis( double gravParameter, double targetPeriod )
        {
            return Math.Cbrt( (gravParameter * (targetPeriod * targetPeriod)) / (4 * (Math.PI * Math.PI)) );
        }

        public double apoapsis => (1 + eccentricity) * semiMajorAxis;
        public double periapsis => (1 - eccentricity) * semiMajorAxis;

        /// <summary>
        /// Calculates the minimum speed (at apoapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetApoapsisSpeed( double gravParameter )
        {
            return Math.Sqrt( ((1 - eccentricity) * gravParameter) / ((1 + eccentricity) * semiMajorAxis) );
        }

        /// <summary>
        /// Calculates the maximum speed (at periapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetPeriapsisSpeed( double gravParameter )
        {
            return Math.Sqrt( ((1 + eccentricity) * gravParameter) / ((1 - eccentricity) * semiMajorAxis) );
        }

        /// <summary>
        /// Calculates the specific orbital energy (a.k.a. vis-viva energy) for a body orbiting another body.
        /// </summary>
        /// <param name="gravParameter">The sum of the standard gravitational parameters of both orbiting bodies, in [m^3/s^2].</param>
        /// <returns>The specific orbital energy, in [J/kg].</returns>
        public double GetSpecificOrbitalEnergy( double gravParameter )
        {
            return -( gravParameter / (2 * semiMajorAxis) );
        }

        public static double OrbitalSpeedAtRadius( double radius, double gravParameter, double semimajorAxis )
        {
            return Math.Sqrt( gravParameter * ((2.0 / radius) - (1.0 / semimajorAxis)) ); // circular orbit.
        }

        public double GetRelativeInclination( KeplerianOrbit otherOrbit )
        {
            //return Vector3Dbl.Angle( GetOrbitNormal(), otherOrbit.GetOrbitNormal() );

            throw new NotImplementedException();
        }

        public void AddAcceleration( Vector3Dbl acceleration )
        {
            throw new NotImplementedException();
        }

        public void AddAccelerationAtUT( Vector3Dbl acceleration, double ut )
        {
            throw new NotImplementedException();
        }

        public StateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( StateVector stateVector )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVectorAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVector( float t )
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetCurrentOrbitalFrame()
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetOrbitalFrameAtUT( double ut )
        {
            // For keplerian, you don't get the directions directly, but first compute the orientation and then get the directions from that.

            throw new NotImplementedException();
        }
    }
}