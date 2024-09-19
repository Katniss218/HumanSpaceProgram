using HSP.CelestialBodies;
using HSP.Trajectories;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Trajectories
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
        /// The semi-major axis of the orbit, in [m].
        /// </summary>
        public double SemiMajorAxis { get; private set; }

        public double SemiMinorAxis => Math.Sqrt( (SemiMajorAxis * SemiMajorAxis) * (1 - (Eccentricity * Eccentricity)) );

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
        public double LongitudeOfAscendingNode { get; private set; }

        /// <summary>
        /// The argument of periapsis of the orbit, in [Rad].
        /// </summary>
        public double ArgumentOfPeriapsis { get; private set; }

        /// <summary>
        /// The mean anomaly of the orbiting object, at <see cref="epoch"/>, in [Rad]. <br />
        /// Mean anomaly is the angle between the periapsis, the central body, and the orbiting body, if the orbit was circular and with the same orbital period.
        /// </summary>
        /// <remarks>
        /// This angle can be set outside the standard range [0..2*PI].
        /// </remarks>
        public double MeanAnomaly { get; private set; } // We store the mean anomaly because it is convenient.

        double _cachedTrueAnomaly;

        /// <summary>
        /// The reference epoch for the orbit
        /// </summary>
        //public double epoch { get; private set; }
        // original code has a Vector3 epoch which is calculated as "position on orbit - state vector position" (so, vector.zero??)

        public double UT { get; private set; }

        public double GravitationalParameter { get; set; }

        /// <summary>
        /// The length of half of the chord line perpendicular to the major axis, and passing through the focus.
        /// </summary>
        public double semiLatusRectum => SemiMajorAxis * (1 - (Eccentricity * Eccentricity));

        public double ApoapsisHeight => (1 + Eccentricity) * SemiMajorAxis;
        public double PeriapsisHeight => (1 - Eccentricity) * SemiMajorAxis;

        ITrajectory _parentBody;
        public ITrajectory ParentBody
        {
            get
            {
                if( _parentBody.IsUnityNull() )
                    _parentBody = CelestialBodyManager.Get( ParentBodyID ).GetComponent<TrajectoryTransform>().Trajectory;
                return _parentBody;
            }
        }
        public string ParentBodyID { get; set; }

        public double Mass { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semiMajorAxis"></param>
        /// <param name="eccentricity"></param>
        /// <param name="inclination"></param>
        /// <param name="longitudeOfAscendingNode"></param>
        /// <param name="argumentOfPeriapsis"></param>
        /// <param name="meanAnomaly"></param>
        /// <param name="epoch"></param>
        /// <param name="mass">The mass of the object represented by this trajectory.</param>
        public KeplerianOrbit( double ut, string parentBodyId, double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, double epoch, double mass )
        {
            this.UT = ut;
            this.ParentBodyID = parentBodyId;
            this.SemiMajorAxis = semiMajorAxis;
            this.Eccentricity = eccentricity;
            this.Inclination = inclination;
            this.LongitudeOfAscendingNode = longitudeOfAscendingNode;
            this.ArgumentOfPeriapsis = argumentOfPeriapsis;
            this.MeanAnomaly = meanAnomaly;

            //this.epoch = epoch;
            this.GravitationalParameter = GetGravParameter( mass );
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
            return MeanAnomaly / GetMeanMotion();
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

        //
        //
        //

        public TrajectoryBodyState GetCurrentState()
        {
#warning TODO - using ITrajectory directly MIGHT introduce the same synchronization problem LATER.
            TrajectoryBodyState parentState = ParentBody.GetCurrentState();

            Vector3Dbl bodyCentricPosition = CalculatePosition( _cachedTrueAnomaly, semiLatusRectum, LongitudeOfAscendingNode, ArgumentOfPeriapsis, Inclination );
            Vector3Dbl velocity = CalculateVelocity( _cachedTrueAnomaly, GravitationalParameter, semiLatusRectum, LongitudeOfAscendingNode, ArgumentOfPeriapsis, Inclination, Eccentricity );
            Vector3Dbl acceleration = bodyCentricPosition.normalized * PhysicalConstants.G * (parentState.Mass / bodyCentricPosition.sqrMagnitude);

            return new TrajectoryBodyState( bodyCentricPosition + parentState.AbsolutePosition, velocity + parentState.AbsoluteVelocity, acceleration + parentState.AbsoluteAcceleration, Mass );
        }

        public void SetCurrentState( TrajectoryBodyState stateVector )
        {
            TrajectoryBodyState parentState = ParentBody.GetCurrentState();
            Vector3Dbl bodyCentricPosition = stateVector.AbsolutePosition - parentState.AbsolutePosition;
            Vector3Dbl bodyCentricVelocity = stateVector.AbsoluteVelocity - parentState.AbsoluteVelocity;
#warning TODO - stuck in infinite loop.
            return;
#warning TODO - using ITrajectory directly MIGHT introduce the same synchronization problem LATER.

            //Vector3Dbl acceleration = stateVector.AbsoluteAcceleration - parentBodyAcceleration;
            (double eccentricity, double semiMajorAxis, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double trueAnomaly)
                = CalcFromPosVel( GravitationalParameter, bodyCentricPosition, bodyCentricVelocity );

            Eccentricity = eccentricity;
            SemiMajorAxis = semiMajorAxis;
            Inclination = inclination;
            LongitudeOfAscendingNode = longitudeOfAscendingNode;
            ArgumentOfPeriapsis = argumentOfPeriapsis;
            _cachedTrueAnomaly = trueAnomaly;
            MeanAnomaly = MeanAnomaly;
            Mass = stateVector.Mass;
            // keplerian from state vector.
            throw new NotImplementedException();
        }

        public TrajectoryBodyState GetStateAtUT( double ut )
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

        public bool HasCacheForUT( double ut )
        {
            // Keplerian orbits have a closed form solution, so they can be calculated arbitrarily far ahead in an instant.

#warning TODO - change to 'true' when the on-demand calculation is ready.
            return false;
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            double meanMotion = GetMeanMotion();
            MeanAnomaly += meanMotion * dt;
            UT += dt;

            _cachedTrueAnomaly = CalculateTrueAnomaly( this.MeanAnomaly, this.Eccentricity );
        }



        //
        //
        //

        public static Vector3Dbl CalculatePosition(
            double trueAnomaly, double semiLatusRectum,
            double longitudeOfAscendingNode, double argumentOfPeriapsis, double inclination )
        {
            double p = semiLatusRectum;

            Vector3Dbl position = new Vector3Dbl(
                p * (Math.Cos( longitudeOfAscendingNode ) * Math.Cos( argumentOfPeriapsis + trueAnomaly ) - Math.Sin( longitudeOfAscendingNode ) * Math.Cos( inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )),
                p * (Math.Sin( longitudeOfAscendingNode ) * Math.Cos( argumentOfPeriapsis + trueAnomaly ) + Math.Cos( longitudeOfAscendingNode ) * Math.Cos( inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )),
                p * Math.Sin( inclination ) * Math.Sin( argumentOfPeriapsis + trueAnomaly )
            );

#warning TODO - original has a Vector3 epoch, what is that?
            return position /* - epoch */;
        }

        public static Vector3Dbl CalculateVelocity(
            double trueAnomaly, double gravitationalParameter, double semiLatusRectum,
            double longitudeOfAscendingNode, double argumentOfPeriapsis, double inclination, double eccentricity )
        {
            double g = -Math.Sqrt( gravitationalParameter / semiLatusRectum );

            Vector3Dbl velocity = new Vector3Dbl(
                g * (Math.Cos( longitudeOfAscendingNode ) * (Math.Sin( argumentOfPeriapsis + trueAnomaly ) + eccentricity * Math.Sin( argumentOfPeriapsis )) +
                        Math.Sin( longitudeOfAscendingNode ) * Math.Cos( inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + eccentricity * Math.Cos( argumentOfPeriapsis ))),
                g * (Math.Sin( longitudeOfAscendingNode ) * (Math.Sin( argumentOfPeriapsis + trueAnomaly ) + eccentricity * Math.Sin( argumentOfPeriapsis )) -
                        Math.Cos( longitudeOfAscendingNode ) * Math.Cos( inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + eccentricity * Math.Cos( argumentOfPeriapsis ))),
                g * (Math.Sin( inclination ) * (Math.Cos( argumentOfPeriapsis + trueAnomaly ) + eccentricity * Math.Cos( argumentOfPeriapsis )))
            );

            return velocity;
        }


        private static (double eccentricity, double semiMajorAxis, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double trueAnomaly)
            CalcFromPosVel( double GravitationalParameter, Vector3Dbl stateVectorPosition, Vector3Dbl stateVectorVelocity )
        {
            // vectors in planet-centric absolute frame. (axes aligned with absolute frame)
            double eccentricity;
            double semiMajorAxis;
            double inclination;
            double longitudeOfAscendingNode;
            double argumentOfPeriapsis;
            double trueAnomaly;
            //Vector3Dbl thisepoch;

            // calculate specific relative angular momement
            Vector3Dbl h = Vector3Dbl.Cross( stateVectorPosition, stateVectorVelocity );

            // calculate vector to the ascending node
            Vector3Dbl n = new Vector3Dbl( -h.y, h.x, 0 );

            // calculate eccentricity vector and scalar
            Vector3Dbl e = (Vector3Dbl.Cross( stateVectorVelocity, h ) * (1.0 / GravitationalParameter)) - (stateVectorPosition * (1.0 / stateVectorPosition.magnitude));
            eccentricity = e.magnitude;

            // calculate specific orbital energy and semi-major axis
            double E = stateVectorVelocity.sqrMagnitude * 0.5 - GravitationalParameter / stateVectorPosition.magnitude;
            semiMajorAxis = -GravitationalParameter / (E * 2);

            // calculate inclination
            inclination = Math.Acos( h.z / h.magnitude );

            // calculate longitude of ascending node
            if( inclination == 0.0 )
                longitudeOfAscendingNode = 0;
            else if( n.y >= 0.0 )
                longitudeOfAscendingNode = Math.Acos( n.x / n.magnitude );
            else
                longitudeOfAscendingNode = 2 * Math.PI - Math.Acos( n.x / n.magnitude );

            // calculate argument of periapsis
            if( inclination == 0.0 )
                argumentOfPeriapsis = Math.Acos( e.x / e.magnitude );
            else if( e.z >= 0.0 )
                argumentOfPeriapsis = Math.Acos( Vector3Dbl.Dot( n, e ) / (n.magnitude * e.magnitude) );
            else
                argumentOfPeriapsis = 2 * Math.PI - Math.Acos( Vector3Dbl.Dot( n, e ) / (n.magnitude * e.magnitude) );

            // calculate true anomaly
            if( Vector3Dbl.Dot( stateVectorPosition, stateVectorVelocity ) >= 0.0 )
                trueAnomaly = Math.Acos( Vector3Dbl.Dot( e, stateVectorPosition ) / (e.magnitude * stateVectorPosition.magnitude) );
            else
                trueAnomaly = 2 * Math.PI - Math.Acos( Vector3Dbl.Dot( e, stateVectorPosition ) / (e.magnitude * stateVectorPosition.magnitude) );

            // calculate epoch
            //thisepoch = new Vector3Dbl( 0, 0, 0 );
            //thisepoch = CalculatePosition( ... ) - stateVectorPosition;
            return (eccentricity, semiMajorAxis, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, trueAnomaly);
        }


        public static double CalculateTrueAnomaly( double meanAnomaly, double eccentricity )
        {
            double eccentricAnomaly = CalculateEccentricAnomaly( meanAnomaly, eccentricity );
            double trueAnomaly = CalculateTrueAnomalyFromEccentric( eccentricAnomaly, eccentricity );
            return trueAnomaly;
        }

        // For small eccentricities a good approximation of true anomaly can be 
        // obtained by the following formula (the error is of the order e^3)
        private static double EstimateTrueAnomalyLowEccentricity( double meanAnomaly, double eccentricity )
        {
            double M = meanAnomaly;
            return M + 2 * eccentricity * Math.Sin( M ) + 1.25 * eccentricity * eccentricity * Math.Sin( 2 * M );
        }

        private static double CalculateEccentricAnomaly( double meanAnomaly, double eccentricity )
        {
            double estTrueAnomaly = EstimateTrueAnomalyLowEccentricity( meanAnomaly, eccentricity );
            double E = Math.Acos( (eccentricity + Math.Cos( estTrueAnomaly )) / (1.0 + eccentricity * Math.Cos( estTrueAnomaly )) );
            double M = E - eccentricity * Math.Sin( E );

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
                M = E - eccentricity * Math.Sin( E );
            }

            if( meanAnomaly > Math.PI && E < Math.PI )
                E = 2 * Math.PI - E;

            return E;
        }

        private static double CalculateTrueAnomalyFromEccentric( double eccentricAnomaly, double eccentricity )
        {
            double trueAnomaly = Math.Acos( (Math.Cos( eccentricAnomaly ) - eccentricity) / (1.0 - eccentricity * Math.Cos( eccentricAnomaly )) );

            if( eccentricAnomaly > Math.PI && trueAnomaly < Math.PI )
                trueAnomaly = (2.0 * Math.PI) - trueAnomaly;

            return trueAnomaly;
        }


        private static double EccentricAnomalyFromTrueAnomaly( double trueAnomaly, double eccentricity )
        {
            double E = Math.Acos( (eccentricity + Math.Cos( trueAnomaly )) / (1 + eccentricity * Math.Cos( trueAnomaly )) );
            if( trueAnomaly > Math.PI && E < Math.PI )
                E = 2 * Math.PI - E;
            return E;
        }

        /// <summary>
        /// calculates the standard gravitational parameter for a celestial body with the specified mass.
        /// </summary>
        /// <param name="bodyMass">The mass of the celestial body, in [kg].</param>
        /// <returns>The standard gravitational parameter for a body with the given mass, in [m^3/s^2].</returns>
        public static double GetGravParameter( double bodyMass )
        {
            return PhysicalConstants.G * bodyMass;
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


        [MapsInheritingFrom( typeof( KeplerianOrbit ) )]
        public static SerializationMapping KeplerianOrbitMapping()
        {
            return new MemberwiseSerializationMapping<KeplerianOrbit>()
            {
                ("ut", new Member<KeplerianOrbit, double>( o => o.UT )),
                ("mass", new Member<KeplerianOrbit, double>( o => o.Mass )),
                ("parent_body", new Member<KeplerianOrbit, string>( o => o.ParentBodyID )),
                ("semi_major_axis", new Member<KeplerianOrbit, double>( o => o.SemiMajorAxis )),
            };
        }
    }
}