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

        double? _cachedTrueAnomaly;
        public double TrueAnomaly
        {
            get
            {
                if( !_cachedTrueAnomaly.HasValue )
                    _cachedTrueAnomaly = CalculateTrueAnomaly( MeanAnomaly, Eccentricity );
                return _cachedTrueAnomaly.Value;
            }
        }

        public double UT { get; private set; }

        /// <summary>
        /// The length of half of the chord line perpendicular to the major axis, and passing through the focus.
        /// </summary>
        public double semiLatusRectum => SemiMajorAxis * (1 - (Eccentricity * Eccentricity));

        public double ApoapsisHeight => (1 + Eccentricity) * SemiMajorAxis;
        public double PeriapsisHeight => (1 - Eccentricity) * SemiMajorAxis;

        TrajectoryTransform _parentBodyPointer;
        ITrajectory _parentBody;
        /// <summary>
        /// Gets or sets the parent trajectory directly. <br/>
        /// May be null.
        /// </summary>
        public ITrajectory ParentBody
        {
            get
            {
                if( _parentBody == null && ParentBodyID != null )
                {
                    if( _parentBodyPointer == null )
                    {
                        var body = CelestialBodyManager.Get( ParentBodyID );
                        if( body == null )
                        {
                            Debug.LogError( $"Couldn't find a body with ID '{ParentBodyID}'." );
                        }
                        _parentBodyPointer = body.GetComponent<TrajectoryTransform>();
                        if( _parentBodyPointer == null )
                        {
                            Debug.LogError( $"The body with ID '{ParentBodyID}' doesn't have a trajectory transform." );
                        }
                    }
                    _parentBody = _parentBodyPointer.Trajectory;
                }
                return _parentBody;
            }
            set
            {
                _parentBody = value;
                _parentBodyPointer = null;
            }
        }
        public string ParentBodyID { get; set; }

        public double Mass { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ut">The current (initial) univeral time.</param>
        /// <param name="semiMajorAxis"></param>
        /// <param name="eccentricity"></param>
        /// <param name="inclination"></param>
        /// <param name="longitudeOfAscendingNode"></param>
        /// <param name="argumentOfPeriapsis"></param>
        /// <param name="initialMeanAnomaly">The initial mean anomaly (at <paramref name="ut"/>).</param>
        /// <param name="mass">The mass of the object represented by this trajectory.</param>
        public KeplerianOrbit( double ut, string parentBodyId, double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double initialMeanAnomaly, double mass )
        {
            if( mass <= 0 )
                throw new ArgumentOutOfRangeException( nameof( mass ), $"Mass must be positive." );
            if( eccentricity > 1 && semiMajorAxis > 0 )
                throw new ArgumentOutOfRangeException( nameof( semiMajorAxis ), $"Semi-Major Axis must be negative for hyperbolic orbits (Eccentricity > 1)." );

            this.UT = ut;
            this.ParentBodyID = parentBodyId;
            this.SemiMajorAxis = semiMajorAxis;
            this.Eccentricity = eccentricity;
            if( inclination == 0 )
            {
                this.Inclination = 0;
                this.LongitudeOfAscendingNode = 0;
            }
            else if( inclination < 0 )
            {
                this.Inclination = -inclination;
                this.LongitudeOfAscendingNode = NormalizeAngle( longitudeOfAscendingNode + Math.PI );
                initialMeanAnomaly -= Math.PI;
            }
            else
            {
                this.Inclination = inclination;
                this.LongitudeOfAscendingNode = NormalizeAngle( longitudeOfAscendingNode );
            }
            if( eccentricity < 0.000000001 )
            {
                initialMeanAnomaly += argumentOfPeriapsis;
                this.ArgumentOfPeriapsis = 0;
            }
            else
            {
                this.ArgumentOfPeriapsis = NormalizeAngle( argumentOfPeriapsis );
            }
            this.MeanAnomaly = NormalizeAngle( initialMeanAnomaly );
        }

        /// <summary>
        /// Returns the orbital mean motion for a circular orbit of the same orbital period. <br />
        /// This can be used to calculate the mean anomaly at a different time.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The mean motion, in [Rad/s].</returns>
        public double GetMeanMotion()
        {
            double gravParam = GetGravParameter( ParentBody.Mass );
            return Math.Sqrt( gravParam / Math.Abs( SemiMajorAxis * SemiMajorAxis * SemiMajorAxis ) );
        }

        /// <summary>
        /// Calculates the orbital period of this orbit, around a given body.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The orbital period, in [s].</returns>
        public double GetOrbitalPeriod()
        {
            double gravParam = GetGravParameter( ParentBody.Mass );
            return (2 * Math.PI) * Math.Sqrt( (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis) / gravParam );
        }

        /// <summary>
        /// Calculates the minimum speed (at apoapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetSpeedAtApoapsis()
        {
            double gravParam = GetGravParameter( ParentBody.Mass );
            return Math.Sqrt( ((1 - Eccentricity) * gravParam) / ((1 + Eccentricity) * SemiMajorAxis) );
        }

        /// <summary>
        /// Calculates the maximum speed (at periapsis) of a given circular or elliptical orbit.
        /// </summary>
        /// <param name="gravParameter">The standard gravitational parameter for the parent body, in [m^3/s^2].</param>
        /// <returns>The speed, in [m/s].</returns>
        public double GetSpeedAtPeriapsis()
        {
            double gravParam = GetGravParameter( ParentBody.Mass );
            return Math.Sqrt( ((1 + Eccentricity) * gravParam) / ((1 - Eccentricity) * SemiMajorAxis) );
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
            double gravParam = GetGravParameter( ParentBody.Mass );
            return -(gravParam / (2 * SemiMajorAxis));
        }

        public double GetRelativeInclination( KeplerianOrbit otherOrbit )
        {
            return Vector3.Angle( GetCurrentOrbitalFrame().GetNormal(), otherOrbit.GetCurrentOrbitalFrame().GetNormal() );
        }

        //
        //
        //

        public TrajectoryBodyState GetCurrentState()
        {
            TrajectoryBodyState parentState = ParentBody?.GetCurrentState() ?? new TrajectoryBodyState();

            Vector3Dbl bodyCentricPosition = GetPosition();
            Vector3Dbl velocity = GetVelocity();
            Vector3Dbl acceleration = bodyCentricPosition.normalized * PhysicalConstants.G * (parentState.Mass / bodyCentricPosition.sqrMagnitude);

#warning TODO - if the parent is not updated before 'this', the position will be different to if it was.
            // It won't accumulate, but will be consistently offset proportionally to the velocity of the parent.

            return new TrajectoryBodyState( bodyCentricPosition + parentState.AbsolutePosition, velocity + parentState.AbsoluteVelocity, acceleration + parentState.AbsoluteAcceleration, Mass );
        }

        public void SetCurrentState( TrajectoryBodyState stateVector )
        {
            TrajectoryBodyState parentState = ParentBody?.GetCurrentState() ?? new TrajectoryBodyState();
            Vector3Dbl bodyCentricPosition = stateVector.AbsolutePosition - parentState.AbsolutePosition;
            Vector3Dbl bodyCentricVelocity = stateVector.AbsoluteVelocity - parentState.AbsoluteVelocity;

            double gravParam = GetGravParameter( ParentBody.Mass );
            (double eccentricity, double semiMajorAxis, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double trueAnomaly)
                = CalculateFromStateVector( gravParam, bodyCentricPosition, bodyCentricVelocity );
            double eccAnom = EccentricAnomalyFromTrueAnomaly( trueAnomaly, eccentricity );
            double meanAnom = MeanAnomalyFromEccentricAnomaly( eccAnom, eccentricity );

            Eccentricity = eccentricity;
            SemiMajorAxis = semiMajorAxis;
            Inclination = inclination;
            LongitudeOfAscendingNode = longitudeOfAscendingNode;
            ArgumentOfPeriapsis = argumentOfPeriapsis;
            _cachedTrueAnomaly = trueAnomaly;
            MeanAnomaly = meanAnom;
            Mass = stateVector.Mass;
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
            // Figure out how to do on demand if the parent is not cacheable.
            // - It is possible, but code it nicely

            return false;
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            double meanMotion = GetMeanMotion();

            MeanAnomaly += (meanMotion * dt);
            MeanAnomaly = NormalizeAngle( MeanAnomaly );
            UT += dt;

            _cachedTrueAnomaly = null; // Invalidate the cache
        }



        //
        //
        //

        // An orbit with eccentricity = 1 is always a straight line (with tangential velocity component equal to 0).
        // stepping an orbit with eccentricity = 1 might be tricky.

        // hyperbolic (and parabolic) orbits have negative semimajor axis because if it's positive, it causes h (specific angular momentum) to be NaN

        // ecc=1 orbits have 2 cases - elliptical and hyperbolic, depending on initial velocity


        public Vector3Dbl GetPosition()
        {
            double trueAnomaly = TrueAnomaly;

            // Distance to body
            double r = (SemiMajorAxis * (1 - (Eccentricity * Eccentricity))) / (1 + Eccentricity * Math.Cos( trueAnomaly ));

            // Position in the perifocal frame
            double xOrbital = r * Math.Cos( trueAnomaly );
            double yOrbital = r * Math.Sin( trueAnomaly );
            double zOrbital = 0;

            Vector3Dbl positionInOrbitalPlane = new Vector3Dbl( xOrbital, yOrbital, zOrbital );

            positionInOrbitalPlane = RotateZ( positionInOrbitalPlane, ArgumentOfPeriapsis );
            positionInOrbitalPlane = RotateX( positionInOrbitalPlane, Inclination );
            positionInOrbitalPlane = RotateZ( positionInOrbitalPlane, LongitudeOfAscendingNode );

            return positionInOrbitalPlane;
        }

        public Vector3Dbl GetVelocity()
        {
            double trueAnomaly = TrueAnomaly;
            double gravParam = GetGravParameter( ParentBody.Mass );

            // Specific angular momentum
            double h = Math.Sqrt( SemiMajorAxis * (1 - Eccentricity * Eccentricity) * gravParam );
#warning TODO - when the object velocity is 0, the eccentricity is 1, and `h` is 0. when velociy is 0 we should assume it's at apoapsis of a perfectly linear orbit

            // Radial and tangential velocity components in the orbital plane
            double vr = (gravParam / h) * Eccentricity * Math.Sin( trueAnomaly );
            double vtheta = (gravParam / h) * (1 + Eccentricity * Math.Cos( trueAnomaly ));

            // Velocity in the perifocal frame
            double vxOrbital = vr * Math.Cos( trueAnomaly ) - vtheta * Math.Sin( trueAnomaly );
            double vyOrbital = vr * Math.Sin( trueAnomaly ) + vtheta * Math.Cos( trueAnomaly );
            double vzOrbital = 0;

            Vector3Dbl velocityInOrbitalPlane = new Vector3Dbl( vxOrbital, vyOrbital, vzOrbital );

            velocityInOrbitalPlane = RotateZ( velocityInOrbitalPlane, ArgumentOfPeriapsis );
            velocityInOrbitalPlane = RotateX( velocityInOrbitalPlane, Inclination );
            velocityInOrbitalPlane = RotateZ( velocityInOrbitalPlane, LongitudeOfAscendingNode );

            return velocityInOrbitalPlane;
        }

        private static Vector3Dbl RotateZ( Vector3Dbl position, double angle )
        {
            double cosAngle = Math.Cos( angle );
            double sinAngle = Math.Sin( angle );

            double newX = (cosAngle * position.x) - (sinAngle * position.y);
            double newY = (sinAngle * position.x) + (cosAngle * position.y);

            return new Vector3Dbl( newX, newY, position.z );
        }

        private static Vector3Dbl RotateX( Vector3Dbl position, double angle )
        {
            double cosAngle = Math.Cos( angle );
            double sinAngle = Math.Sin( angle );

            double newY = (cosAngle * position.y) - (sinAngle * position.z);
            double newZ = (sinAngle * position.y) + (cosAngle * position.z);

            return new Vector3Dbl( position.x, newY, newZ );
        }

        private static (double eccentricity, double semiMajorAxis, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double trueAnomaly)
            CalculateFromStateVector( double gravitationalParameter, Vector3Dbl stateVectorPosition, Vector3Dbl stateVectorVelocity )
        {
            // Specific angular momentum vector (angular momentum of the orbiting body divided by its mass).
            // This vector is always perpendicular to the orbital plane.
            Vector3Dbl h = Vector3Dbl.Cross( stateVectorPosition, stateVectorVelocity );

            // Vector from focus towards the ascending node.
            Vector3Dbl n = new Vector3Dbl( -h.y, h.x, 0.0 );

            // Eccentricity vector is the vector pointing from apoapsis towards periapsis, with its magnitude equal to the eccentricity.
            Vector3Dbl e = (Vector3Dbl.Cross( stateVectorVelocity, h ) / gravitationalParameter) - (stateVectorPosition / stateVectorPosition.magnitude);

            double semiMajorAxis = 1.0 / (2.0 / stateVectorPosition.magnitude - (stateVectorVelocity.magnitude * stateVectorVelocity.magnitude) / gravitationalParameter);

            double eccentricity = e.magnitude;

            double inclination = Math.Acos( h.z / h.magnitude );

            double longitudeOfAscendingNode;
            if( inclination < 1e-6 || inclination > (Math.PI - 1e-6) )
                longitudeOfAscendingNode = 0;
            else
            {
                /*
                if( n.y >= 0.0 )
                    longitudeOfAscendingNode = Math.Acos( n.x / n.magnitude );
                else
                    longitudeOfAscendingNode = (2 * Math.PI) - Math.Acos( n.x / n.magnitude );
                */

                longitudeOfAscendingNode = Math.Atan2( h.x, -h.y );
                longitudeOfAscendingNode = NormalizeAngle( longitudeOfAscendingNode );
            }

            double argumentOfPeriapsis;
            if( eccentricity < 1e-6 )
                argumentOfPeriapsis = 0;
            else
            {
                if( inclination < 1e-6 || inclination > (Math.PI - 1e-6) )
                    if( h.z >= 0 )
                        argumentOfPeriapsis = Math.Atan2( e.y, e.x ); // y and x being in opposite order to how atan2 wants them is deliberate.
                    else
                        argumentOfPeriapsis = (2.0 * Math.PI) - Math.Atan2( e.y, e.x ); // y and x being in opposite order to how atan2 wants them is deliberate.
                else
                {
                    if( e.z >= 0.0 )
                        argumentOfPeriapsis = Math.Acos( Vector3Dbl.Dot( n, e ) / (n.magnitude * e.magnitude) );
                    else
                        argumentOfPeriapsis = (2.0 * Math.PI) - Math.Acos( Vector3Dbl.Dot( n, e ) / (n.magnitude * e.magnitude) );
                }
                argumentOfPeriapsis = NormalizeAngle( argumentOfPeriapsis );
            }

            double trueAnomaly;
            if( eccentricity < 1e-6 )
            {
                // Since a circular orbit doesn't have a uniquely determined periapsis, the eccentricity vector can't be used to determine the anomaly.
                if( inclination < 1e-6 || inclination > (Math.PI - 1e-6) )
                {
                    // For non-inclined circular orbits, we use the angle between the position and reference direction (true longitude).
                    if( stateVectorVelocity.x <= 0 )
                        trueAnomaly = Math.Acos( stateVectorPosition.x / stateVectorPosition.magnitude );
                    else
                        trueAnomaly = (2.0 * Math.PI) - Math.Acos( stateVectorPosition.x / stateVectorPosition.magnitude );
                }
                else
                {
                    // For inclined circular orbits, we use the angle between the ascending node and the position (argument of latitude).
                    if( stateVectorPosition.z >= 0 )
                        trueAnomaly = Math.Acos( Vector3Dbl.Dot( n, stateVectorPosition ) / (n.magnitude * stateVectorPosition.magnitude) );
                    else
                        trueAnomaly = (2.0 * Math.PI) - Math.Acos( Vector3Dbl.Dot( n, stateVectorPosition ) / (n.magnitude * stateVectorPosition.magnitude) );
                }
            }
            else
            {
                //if( Vector3Dbl.Dot( stateVectorPosition, stateVectorVelocity ) >= 0.0 )
                //    trueAnomaly = Math.Acos( Vector3Dbl.Dot( e, stateVectorPosition ) / (e.magnitude * stateVectorPosition.magnitude) );
                //else
                //    trueAnomaly = (2 * Math.PI) - Math.Acos( Vector3Dbl.Dot( e, stateVectorPosition ) / (e.magnitude * stateVectorPosition.magnitude) );

                // For most orbits (inclined and non-circular) we use the standard equation for mean anomaly.
                // This variant appears to be more numerically stable for large orbits.
                var posDotvel = Vector3Dbl.Dot( stateVectorPosition, stateVectorVelocity );
                double trueAnomalyX = ((h.magnitude * h.magnitude) / (stateVectorPosition.magnitude * gravitationalParameter)) - 1.0;
                double trueAnomalyY = (h.magnitude * posDotvel) / (stateVectorPosition.magnitude * gravitationalParameter);
                trueAnomaly = Math.Atan2( trueAnomalyY, trueAnomalyX );
            }
            trueAnomaly = NormalizeAngle( trueAnomaly );

            return (eccentricity, semiMajorAxis, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, trueAnomaly);
        }

        public static double NormalizeAngle( double radians )
        {
            const double twoPi = 2.0 * Math.PI;

            double normalized = radians % twoPi;

            if( normalized < 0 )
                normalized += twoPi;

            return normalized;
        }

        public static double CalculateTrueAnomaly( double meanAnomaly, double eccentricity )
        {
            meanAnomaly = NormalizeAngle( meanAnomaly );
            double eccentricAnomaly = CalculateEccentricAnomaly( meanAnomaly, eccentricity );
            double trueAnomaly = CalculateTrueAnomalyFromEccentric( eccentricAnomaly, eccentricity );

            return trueAnomaly;
        }

        private static double CalculateEccentricAnomaly( double meanAnomaly, double eccentricity )
        {
            if( meanAnomaly == 0 )
                return 0;

            const double maxError = 1e-10;

            // Initial guess
            double eccentricAnomaly = meanAnomaly + (eccentricity * Math.Sin( meanAnomaly )) + (0.5 * (eccentricity * eccentricity) * Math.Sin( 2.0 * meanAnomaly ));
            double error = 1.0;

            while( Math.Abs( error ) > maxError )
            {
                double step = eccentricAnomaly - (eccentricity * Math.Sin( eccentricAnomaly ));

                error = (meanAnomaly - step) / (1.0 - (eccentricity * Math.Cos( eccentricAnomaly )));

                eccentricAnomaly += error;
            }

            return eccentricAnomaly;
        }

        private static double CalculateTrueAnomalyFromEccentric( double eccentricAnomaly, double eccentricity )
        {
            if( eccentricAnomaly == 0 )
                return 0;

            if( eccentricity >= 1.0 )
            {
                double coshE = Math.Sinh( eccentricAnomaly / 2.0 );
                double sinhE = Math.Cosh( eccentricAnomaly / 2.0 );
                return 2.0 * Math.Atan2( Math.Sqrt( eccentricity + 1.0 ) * coshE, Math.Sqrt( eccentricity - 1.0 ) * sinhE );
            }

            double cosE = Math.Cos( eccentricAnomaly / 2.0 );
            double sinE = Math.Sin( eccentricAnomaly / 2.0 );
            return 2.0 * Math.Atan2( Math.Sqrt( 1.0 + eccentricity ) * sinE, Math.Sqrt( 1.0 - eccentricity ) * cosE );
        }


        private static double EccentricAnomalyFromTrueAnomaly( double trueAnomaly, double eccentricity )
        {
            if( eccentricity == 0 )
                return 0;

            double cosT = Math.Cos( trueAnomaly / 2.0 );
            double sinT = Math.Sin( trueAnomaly / 2.0 );
            if( eccentricity < 1.0 )
            {
                return 2.0 * Math.Atan2( Math.Sqrt( 1.0 - eccentricity ) * sinT, Math.Sqrt( 1.0 + eccentricity ) * cosT );
            }

            double root = Math.Sqrt( (eccentricity - 1.0) / (eccentricity + 1.0) ) * sinT / cosT;

            return Math.Log( (1.0 + root) / (1.0 - root) );
        }

        private static double MeanAnomalyFromEccentricAnomaly( double eccentricAnomaly, double eccentricity )
        {
            if( eccentricity >= 1.0 )
            {
                return eccentricity * Math.Sinh( eccentricAnomaly ) - eccentricAnomaly;
            }
            return eccentricAnomaly - eccentricity * Math.Sin( eccentricAnomaly )
                % Math.PI;
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