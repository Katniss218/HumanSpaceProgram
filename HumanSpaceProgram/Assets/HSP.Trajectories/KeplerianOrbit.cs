using HSP.Trajectories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Core
{
    /// <summary>
    /// A simulated trajectory that follows kepler's laws of planetary motion.
    /// </summary>
    public class KeplerianOrbit : ITrajectory
    {
        // Sources:
        // https://github.com/MuMech/MechJeb2/blob/dev/MechJeb2/MechJebLib/Core/Maths.cs#L226
        // https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf
        // https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
        // https://gist.github.com/triffid/166b8f5597ce52bcafd15a18218b066f

        /// <summary>
        /// The value of the gravitational constant, in [m^3/kg/s^2].
        /// </summary>
        public const double G = 6.6743e-11;

        /// <summary>
        /// The semi-major axis of the orbit, in [m].
        /// </summary>
        public double SemiMajorAxis { get; private set; }

        /// <summary>
        /// The eccentricity of the orbit, in [0] for circular, [0..1] for elliptical, [1] for parabolic, [1..] for hyperbolic.
        /// </summary>
        public double Eccentricity { get; private set; }

        /// <summary>
        /// The inclination of the orbit, in [Rad].
        /// </summary>
        public double Inclination { get; private set; }

        /// <summary>
        /// The longitude of the ascending node of the orbit, in [Rad].
        /// </summary>
        public double longitudeOfAscendingNode { get; private set; }

        /// <summary>
        /// The argument of periapsis of the orbit, in [Rad].
        /// </summary>
        public double argumentOfPeriapsis { get; private set; }

        /// <summary>
        /// The mean anomaly of the orbiting object, at <see cref="epoch"/>, in [Rad]. <br />
        /// Mean anomaly is the angle between the periapsis, the central body, and the orbiting body, if the orbit was circular and with the same orbital period.
        /// </summary>
        /// <remarks>
        /// This angle can be set outside the standard range [0..2*PI].
        /// </remarks>
        public double meanAnomaly { get; private set; } // We store the mean anomaly because it is convenient.

        double _cachedTrueAnomaly;

        /// <summary>
        /// The reference epoch for the orbit, UT, in [s].
        /// </summary>
        public double epoch { get; private set; } // referenceUT

        public double UT { get; private set; }

        public double GravitationalParameter { get; set; }

        /// <summary>
        /// The length of half of the chord line perpendicular to the major axis, and passing through the focus.
        /// </summary>
        public double semiLatusRectum => SemiMajorAxis * (1 - (Eccentricity * Eccentricity));

        public double ApoapsisHeight => (1 + Eccentricity) * SemiMajorAxis;
        public double PeriapsisHeight => (1 - Eccentricity) * SemiMajorAxis;

        public Vector3Dbl AbsolutePosition => throw new NotImplementedException();

        public Vector3Dbl AbsoluteVelocity => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => throw new NotImplementedException();

        public double Mass => throw new NotImplementedException();

        public KeplerianOrbit( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, double epoch, double celestialBodyMass )
        {
            this.SemiMajorAxis = semiMajorAxis;
            this.Eccentricity = eccentricity;
            this.Inclination = inclination;
            this.longitudeOfAscendingNode = longitudeOfAscendingNode;
            this.argumentOfPeriapsis = argumentOfPeriapsis;
            this.meanAnomaly = meanAnomaly;

            this.epoch = epoch;
            this.GravitationalParameter = GetGravParameter( celestialBodyMass );
        }

        // Orbits are always in body-centric frame, with the coordinate axes aligned with the absolute frame.
        // State vectors too.

        /// <summary>
        /// Returns the orbital mean motion for a circular orbit of the same orbital period. <br />
        /// This can be used to calculate the mean anomaly at a different time.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The mean motion, in [Rad/s].</returns>
        public double GetMeanMotion()
        {
            return Math.Sqrt( GravitationalParameter / Math.Abs( SemiMajorAxis * SemiMajorAxis * SemiMajorAxis ) );
        }

        /// <summary>
        /// Calculates the orbital period of this orbit, around a given body.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The orbital period, in [s].</returns>
        public double GetOrbitalPeriod()
        {
            return (2 * Math.PI) * Math.Sqrt( (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis) / GravitationalParameter );
        }

        /// <summary>
        /// Calculates the minimum speed (at apoapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetSpeedAtApoapsis()
        {
            return Math.Sqrt( ((1 - Eccentricity) * GravitationalParameter) / ((1 + Eccentricity) * SemiMajorAxis) );
        }

        /// <summary>
        /// Calculates the maximum speed (at periapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetSpeedAtPeriapsis()
        {
            return Math.Sqrt( ((1 + Eccentricity) * GravitationalParameter) / ((1 - Eccentricity) * SemiMajorAxis) );
        }

        public double GetTimeSincePeriapsis()
        {
            return meanAnomaly / GetMeanMotion();
        }

        /// <summary>
        /// Calculates the specific orbital energy (a.k.a. vis-viva energy) of this orbit.
        /// </summary>
        /// <param name="gravParameter">The sum of the standard gravitational parameters of both orbiting bodies, in [m^3/s^2].</param>
        /// <returns>The specific orbital energy, in [J/kg].</returns>
        public double GetSpecificOrbitalEnergy()
        {
            return -(GravitationalParameter / (2 * SemiMajorAxis));
        }

        public double GetRelativeInclination( KeplerianOrbit otherOrbit )
        {
            //return Vector3Dbl.Angle( GetOrbitNormal(), otherOrbit.GetOrbitNormal() );

            throw new NotImplementedException();
        }

        public OrbitalStateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( OrbitalStateVector stateVector )
        {
            throw new NotImplementedException();
        }

        public OrbitalStateVector GetStateVectorAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public OrbitalStateVector GetStateVector( float t )
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

        private Vector3Dbl CalculatePosition()
        {
            double trueAnomaly = _cachedTrueAnomaly;
            double p = semiLatusRectum;

            Vector3Dbl position = new Vector3Dbl(
                p * (Math.Cos( longitudeOfAscendingNode ) * Math.Cos( argumentOfPeriapsis + trueAnomaly ) - Math.Sin( longitudeOfAscendingNode ) * Math.Cos( Inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )),
                p * (Math.Sin( longitudeOfAscendingNode ) * Math.Cos( argumentOfPeriapsis + trueAnomaly ) + Math.Cos( longitudeOfAscendingNode ) * Math.Cos( Inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )),
                p * Math.Sin( Inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )
            );

            return position;
        }

        private Vector3Dbl CalculateVelocity()
        {
            double trueAnomaly = _cachedTrueAnomaly;
            double g = -Math.Sqrt( GravitationalParameter / semiLatusRectum );

            Vector3Dbl velocity = new Vector3Dbl(
                g * (Math.Cos( longitudeOfAscendingNode ) * (Math.Sin( argumentOfPeriapsis + trueAnomaly ) + Eccentricity * Math.Sin( argumentOfPeriapsis )) +
                        Math.Sin( longitudeOfAscendingNode ) * Math.Cos( Inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + Eccentricity * Math.Cos( argumentOfPeriapsis ))),
                g * (Math.Sin( longitudeOfAscendingNode ) * (Math.Sin( argumentOfPeriapsis + trueAnomaly ) + Eccentricity * Math.Sin( argumentOfPeriapsis )) -
                        Math.Cos( longitudeOfAscendingNode ) * Math.Cos( Inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + Eccentricity * Math.Cos( argumentOfPeriapsis ))),
                g * (Math.Sin( Inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + Eccentricity * Math.Cos( argumentOfPeriapsis )))
            );

            return velocity;
        }

        // For small eccentricities a good approximation of true anomaly can be 
        // obtained by the following formula (the error is of the order e^3)
        private double estimateTrueAnomaly( double meanAnomaly )
        {
            double M = meanAnomaly;
            return M + 2 * Eccentricity * Math.Sin( M ) + 1.25 * Eccentricity * Eccentricity * Math.Sin( 2 * M );
        }

        double calcEccentricAnomaly()
        {
            double estTrueAnomaly = estimateTrueAnomaly( meanAnomaly );
            double E = Math.Acos( (Eccentricity + Math.Cos( estTrueAnomaly )) / (1.0 + Eccentricity * Math.Cos( estTrueAnomaly )) );
            double M = E - Eccentricity * Math.Sin( E );

            // iterate to get M closer to meanAnomaly
            double rate = 0.01;
            bool lastDec = false;
            while( true )
            {
                if( Math.Abs( M - meanAnomaly ) < 0.0000000000001 )
                    break;
                if( M > meanAnomaly )
                {
                    E -= rate;
                    lastDec = true;
                }
                else
                {
                    E += rate;
                    if( lastDec )
                        rate *= 0.1;
                }
                M = E - Eccentricity * Math.Sin( E );
            }

            if( meanAnomaly > Math.PI && E < Math.PI )
                E = 2 * Math.PI - E;

            return E;
        }

        private double calcTrueAnomaly( double eccentricAnomaly )
        {
            double trueAnomaly = Math.Acos( (Math.Cos( eccentricAnomaly ) - Eccentricity) / (1.0 - Eccentricity * Math.Cos( eccentricAnomaly )) );

            if( eccentricAnomaly > Math.PI && trueAnomaly < Math.PI )
                trueAnomaly = (2.0 * Math.PI) - trueAnomaly;

            return trueAnomaly;
        }

        /// <summary>
        /// calculates the standard gravitational parameter for a celestial body with the specified mass.
        /// </summary>
        /// <param name="bodyMass">The mass of the celestial body, in [kg].</param>
        /// <returns>The standard gravitational parameter for a body with the given mass, in [m^3/s^2].</returns>
        public static double GetGravParameter( double bodyMass )
        {
            return G * bodyMass;
        }

        /// <summary>
        /// Returns the semi-major axis for a circular or elliptical orbit with a given orbital period.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <param name="targetPeriod">The desired period, in [s].</param>
        /// <returns>The semi-major axis, in [m].</returns>
        public static double GetSemiMajorAxis( double bodyMass, double targetPeriod )
        {
            return Math.Cbrt( (GetGravParameter( bodyMass ) * (targetPeriod * targetPeriod)) / (4 * (Math.PI * Math.PI)) );
        }

        /// <summary>
        /// Calculates the escape velocity at a point a certain distance away from the center of gravity.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <param name="radius">The distance from the center of gravity of the parent body, in [m].</param>
        /// <returns>The escape velocity, in [m/s]</returns>
        public static double GetEscapeVelocity( double bodyMass, double radius )
        {
            return Math.Sqrt( (2 * GetGravParameter( bodyMass )) / radius );
        }

        public static double GetOrbitalSpeedForRadius( double radius, double bodyMass, double semimajorAxis )
        {
            return Math.Sqrt( GetGravParameter( bodyMass ) * ((2.0 / radius) - (1.0 / semimajorAxis)) ); // circular orbit.
        }

        public bool HasCacheForUT( double ut )
        {
            // Keplerian orbits have a closed form solution, so they can be calculated arbitrarily far ahead in an instant.

#warning TODO - change to 'true' when the on-demand calculation is ready.
            return false;
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            double meanMotion = GetMeanMotion();
            meanAnomaly += meanMotion * dt;
            UT += dt;

            double eccentricAnomaly = calcEccentricAnomaly();
            _cachedTrueAnomaly = calcTrueAnomaly( eccentricAnomaly );
        }
    }
}