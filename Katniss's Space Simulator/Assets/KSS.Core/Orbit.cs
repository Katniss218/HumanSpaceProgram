using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public struct Orbit
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

        public Orbit( double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, double epoch )
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

        // pricipal directions at UT.

        /// <summary>
        /// Calculates the 'prograde' direction at a given universal time.
        /// </summary>
        /// <param name="UT">The universal time.</param>
        /// <returns>A unit vector pointing prograde.</returns>
        public Vector3 ProgradeAt( double UT )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the 'retrograde' direction at a given universal time.
        /// </summary>
        /// <param name="UT">The universal time.</param>
        /// <returns>A unit vector pointing retrograde.</returns>
        public Vector3 RetrogradeAt( double UT )
        {
            throw new NotImplementedException();
        }

        public Vector3 NormalAt( double UT )
        {
            return GetOrbitNormal();
        }
        public Vector3 AntinormalAt( double UT )
        {
            return -GetOrbitNormal();
        }

        public Vector3 RadialAt( double UT )
        {
            throw new NotImplementedException();
        }
        public Vector3 AntiradialAt( double UT )
        {
            throw new NotImplementedException();
        }

        //

        /// <summary>
        /// Returns the vector normal to the plane of the orbit.
        /// </summary>
        public Vector3 GetOrbitNormal()
        {
            throw new NotImplementedException(); // which way? right hand rule
        }

        public static double OrbitalSpeedAtRadius( double radius, double gravParameter, double semimajorAxis )
        {
            return Math.Sqrt( gravParameter * ((2.0 / radius) - (1.0 / semimajorAxis)) ); // circular orbit.
        }

        public double GetRelativeInclination( Orbit otherOrbit )
        {
            throw new NotImplementedException();
            //return Vector3Dbl.Angle( GetOrbitNormal(), otherOrbit.GetOrbitNormal() );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double TrueAnomalyFromEccentricAnomaly( double eccentricity, double eccentricAnomaly )
        {
            if( eccentricity < 1 )
                return Wrap2PI( 2.0 * Math.Atan( Math.Sqrt( (1 + eccentricity) / (1 - eccentricity) ) * Math.Tan( eccentricAnomaly / 2.0 ) ) );
            if( eccentricity > 1 )
                return Wrap2PI( 2.0 * Math.Atan( Math.Sqrt( (eccentricity + 1) / (eccentricity - 1) ) * Math.Tanh( eccentricAnomaly / 2.0 ) ) );

            return Wrap2PI( 2 * Math.Atan( eccentricAnomaly ) );
        }

        public static (double eccentricAnomaly, double trueAnomaly) AnomaliesFromMean( double meanAnomaly, double eccentricity )
        {
            // Danby's method
            meanAnomaly = meanAnomaly - (2 * Math.PI) * Math.Floor( meanAnomaly / (2 * Math.PI) ); // modulo.
            double eccentricAnomaly;
            double trueAnomaly;

            if( eccentricity == 0 )
            {
                trueAnomaly = meanAnomaly;
                eccentricAnomaly = meanAnomaly;
                return (eccentricAnomaly, trueAnomaly);
            }

            if( eccentricity < 1 )
            {
                // elliptic initial guess
                eccentricAnomaly = meanAnomaly + 0.85 * Math.Sign( Math.Sin( meanAnomaly ) ) * eccentricity;
            }
            else
            {
                // hyperbolic initial guess
                eccentricAnomaly = Math.Log( 2 * meanAnomaly / eccentricity + 1.8 );
            }

            int i = 0;
            const int ITERATIONS = 20;

            while( true )
            {
                double sin, cos, f, fp, fpp, fppp;

                if( eccentricity < 1 )
                {
                    // elliptic
                    sin = eccentricity * Math.Sin( eccentricAnomaly );
                    cos = eccentricity * Math.Cos( eccentricAnomaly );
                    f = eccentricAnomaly - sin - meanAnomaly;
                    fp = 1 - cos;
                    fpp = sin;
                    fppp = cos;
                }
                else
                {
                    // parabolic or hyperbolic
                    sin = eccentricity * Math.Sinh( eccentricAnomaly );
                    cos = eccentricity * Math.Cosh( eccentricAnomaly );
                    f = sin - eccentricAnomaly - meanAnomaly;
                    fp = cos - 1;
                    fpp = sin;
                    fppp = cos;
                }

                i++;
                if( Math.Abs( f ) <= 0.00001 || i > ITERATIONS )
                    break;

                // update eccentric anomaly
                double delta = -f / fp;
                double deltastar = -f / (fp + 0.5 * delta * fpp);
                double deltak = -f / (fp + 0.5 * deltastar * fpp + deltastar * deltastar * fppp / 6);
                eccentricAnomaly += deltak;
            }

            // compute true anomaly
            double sta, cta;

            if( eccentricity < 1 )
            {
                // elliptic
                sta = Math.Sqrt( 1 - eccentricity * eccentricity ) * Math.Sin( eccentricAnomaly );
                cta = Math.Cos( eccentricAnomaly ) - eccentricity;
            }
            else
            {
                // parabolic or hyperbolic
                sta = Math.Sqrt( eccentricity * eccentricity - 1 ) * Math.Sinh( eccentricAnomaly );
                cta = eccentricity - Math.Cosh( eccentricAnomaly );
            }

            trueAnomaly = Math.Atan2( sta, cta );

            return (eccentricAnomaly, trueAnomaly);
        }

        /*public static Orbit FromStateVectors( Vector3Dbl position, Vector3Dbl velocity, double parentBodyMass )
        {
            // https://github.com/RazerM/orbital/blob/0.7.0/orbital/utilities.py#L252

            double gravParameter = G * parentBodyMass;

            Vector3Dbl angularMomentum = Vector3Dbl.Cross( position, velocity ); // h
            Vector3Dbl nodeVector = Vector3Dbl.Cross( Vector3Dbl.forward, angularMomentum );

            double semiLatusRectum = angularMomentum.sqrMagnitude / gravParameter;

            Vector3Dbl eccentricityVector =
                (((velocity.sqrMagnitude - (gravParameter / position.magnitude)) * position) - (Vector3Dbl.Dot( position, velocity ) * velocity)) / gravParameter; // seems good.

            double semimajorAxis;
            double eccentricity;
            double inclination;
            double longitudeOfAscendingNode;
            double argumentOfPeriapsis;
            double meanAnomaly;
            double trueAnomaly;

            eccentricity = eccentricityVector.magnitude;

            double orbitalEnergy = (velocity.sqrMagnitude / 2.0) - (gravParameter / position.magnitude); // seems good.

            if( eccentricity < 1 )
                semimajorAxis = -(gravParameter / (2.0 * orbitalEnergy)); // seems good (for elliptical).
            else
                semimajorAxis = -(semiLatusRectum / (eccentricityVector.sqrMagnitude - 1.0));

            inclination = Math.Acos( angularMomentum.z / angularMomentum.magnitude ); // Inclination is the angle between the angular momentum and its z axis.

            if( Math.Abs( inclination ) < 0.001 )
            {
                longitudeOfAscendingNode = 0;
                if( Math.Abs( eccentricity ) < 0.001 )
                {
                    argumentOfPeriapsis = 0.0;
                    if( eccentricityVector.z < 0.0 )
                    {
                        argumentOfPeriapsis = TwoPI - argumentOfPeriapsis;
                    }

                    trueAnomaly = Math.Acos( position.x / position.magnitude );
                    if( position.x > 0.0 )
                    {
                        trueAnomaly = TwoPI - trueAnomaly;
                    }
                }
                else
                    argumentOfPeriapsis = Math.Acos( eccentricityVector.x / eccentricity );
            }
            else
            {
                longitudeOfAscendingNode = Math.Acos( nodeVector.x / nodeVector.magnitude );
                argumentOfPeriapsis = Math.Acos( Vector3Dbl.Dot( nodeVector, eccentricityVector ) / (nodeVector.magnitude * eccentricity) );
                if( Math.Abs( eccentricity ) < 0.001 )
                {
                    trueAnomaly = Math.Acos( Vector3Dbl.Dot( nodeVector, position ) / (nodeVector.magnitude * position.magnitude) );
                    if( Vector3Dbl.Dot( nodeVector, velocity ) > 0.0 )
                    {
                        trueAnomaly = TwoPI - trueAnomaly;
                    }
                }
            }
            if( Math.Abs( eccentricity ) < 0.001 )
            {
                if( eccentricityVector.z < 0.0 )
                {
                    argumentOfPeriapsis = TwoPI - argumentOfPeriapsis;
                }
            }
            else
            {
                trueAnomaly = Math.Acos( Vector3Dbl.Dot( eccentricityVector, position ) / (eccentricity * position.magnitude) );

                if( Vector3Dbl.Dot( position, velocity ) < 0.0 )
                {
                    trueAnomaly = TwoPI - trueAnomaly;
                }
            }
            meanAnomaly = GetMeanAnomaly( TrueToEccentricAnomaly( trueAnomaly, eccentricity ), eccentricity );

            throw new NotImplementedException();
        }*/

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Wrap2PI( double x )
        {
            x %= (2 * Math.PI);
            return x < 0 ? x + (2 * Math.PI) : x;
        }

        public static Orbit FromStateVectors( double gravParameter, Vector3Dbl position, Vector3Dbl velocity )
        {
            // https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf

            Vector3Dbl momentum = Vector3Dbl.Cross( position, velocity );
            Vector3Dbl eccentricityVector = (Vector3Dbl.Cross( velocity, momentum ) / gravParameter) - (position / position.magnitude);

            Vector3Dbl nodeVector = Vector3Dbl.Cross( Vector3Dbl.forward, momentum ); // points towards the ascending node.

            double trueAnomaly = Math.Acos( Vector3Dbl.Dot( eccentricityVector, position ) / (eccentricityVector.magnitude * position.magnitude) );
            if( Vector3Dbl.Dot( position, velocity ) < 0 )
                trueAnomaly = (2 * Math.PI) - trueAnomaly;

            double inclination = Math.Acos( momentum.z / momentum.magnitude );
            double eccentricity = eccentricityVector.magnitude;
            double eccentricAnomaly = 2 * Math.Atan( Math.Tan( trueAnomaly / 2 ) / Math.Sqrt( (1 + eccentricity) / (1 - eccentricity) ) );

            double longitudeOfAscendingNode = Math.Acos( nodeVector.x / nodeVector.magnitude );
            if( nodeVector.y < 0 )
                longitudeOfAscendingNode = (2 * Math.PI) - longitudeOfAscendingNode;

            double argumentOfPeriapsis = Math.Acos( Vector3Dbl.Dot( nodeVector, eccentricityVector ) / (nodeVector.magnitude * eccentricity) );
            if( eccentricityVector.z < 0 )
                argumentOfPeriapsis = (2 * Math.PI) - argumentOfPeriapsis;

            double meanAnomaly = eccentricAnomaly - eccentricity * Math.Sin( eccentricAnomaly );
            double semiMajorAxis = 1 / ((2 / position.magnitude) - (velocity.sqrMagnitude / gravParameter));

            return new Orbit( semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomaly, TimeManager.UT );

            // Mechjeb version (uses right handed vectors/quaternions)
            /*double positionMagn = position.magnitude;
            double velocityMagn = velocity.magnitude;
            Vector3Dbl dirToPosition = position.normalized;
            var angularVelocity = Vector3Dbl.Cross( position, velocity );
            Vector3Dbl orbitNormal = angularVelocity.normalized;
            Vector3Dbl vtmp = velocity / gravParameter;
            Vector3Dbl eccvec = Vector3Dbl.Cross( vtmp, angularVelocity ) - dirToPosition;

            double semimajorAxis = 1.0 / (2.0 / positionMagn - velocityMagn * velocityMagn / gravParameter);

            //double semiLatusRectum = angularVelocity.sqrMagnitude / gravParameter;

            double d = 1.0 + orbitNormal.z;
            double p = d == 0 ? 0 : orbitNormal.x / d;
            double q = d == 0 ? 0 : -orbitNormal.y / d;

            double const1 = 1.0 / (1.0 + p * p + q * q);

            var fhat = new Vector3Dbl(
                const1 * (1.0 - p * p + q * q),
                const1 * 2.0 * p * q,
                -const1 * 2.0 * p
            );

            var ghat = new Vector3Dbl(
                const1 * 2.0 * p * q,
                const1 * (1.0 + p * p - q * q),
                const1 * 2.0 * q
            );

            double h = Vector3Dbl.Dot( eccvec, ghat );
            double xk = Vector3Dbl.Dot( eccvec, fhat );
            double x1 = Vector3Dbl.Dot( position, fhat );
            double y1 = Vector3Dbl.Dot( position, ghat );
            double xlambdot = Math.Atan2( y1, x1 );

            double eccentricity = Math.Sqrt( h * h + xk * xk );
            double inclination = 2.0 * Math.Atan( Math.Sqrt( p * p + q * q ) );
            double longitudeOfAscendingNode = Wrap2PI( inclination > 0.0001 ? Math.Atan2( p, q ) : 0.0 );
            double argumentOfPeriapsis = Wrap2PI( eccentricity > 0.0001 ? Math.Atan2( h, xk ) - longitudeOfAscendingNode : 0.0 );
            double trueAnomaly = Wrap2PI( xlambdot - longitudeOfAscendingNode - argumentOfPeriapsis );

            return new Orbit( semimajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomaly );*/
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl PerifocalToECIMatrix( double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode )
        {
            return QuaternionDbl.AngleAxis( longitudeOfAscendingNode, Vector3Dbl.forward )
                 * QuaternionDbl.AngleAxis( inclination, Vector3Dbl.right )
                 * QuaternionDbl.AngleAxis( argumentOfPeriapsis, Vector3Dbl.forward );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (Vector3Dbl position, Vector3Dbl velocity) PerifocalFromElements( double gravParameter, double semiLatusRectum, double eccentricity, double trueAnomaly )
        {
            double cosv = Math.Cos( trueAnomaly );
            double sinv = Math.Sin( trueAnomaly );

            Vector3Dbl vPos = new Vector3Dbl( cosv, sinv, 0 );
            Vector3Dbl vVel = new Vector3Dbl( -sinv, eccentricity + cosv, 0 );

            return ((vPos * semiLatusRectum / (1 + eccentricity * cosv)), (vVel * Math.Sqrt( gravParameter / semiLatusRectum )));
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public (Vector3Dbl position, Vector3Dbl velocity) ToStateVectors( double gravParameter )
        {
            // Mechjeb version (uses right handed vectors/quaternions)

            (double trueAnomaly, _) = AnomaliesFromMean( meanAnomaly, eccentricity );

            (Vector3Dbl p, Vector3Dbl q) = PerifocalFromElements( gravParameter, semiLatusRectum, eccentricity, trueAnomaly );
            QuaternionDbl rot = PerifocalToECIMatrix( inclination, argumentOfPeriapsis, longitudeOfAscendingNode );

            return (rot * p, rot * q);
        }

        /*public (Vector3Dbl position, Vector3Dbl velocity) ToStateVectors( double parentBodyMass )
        {
            
            double gravitationalConstant = 6.6743e-11;
            double parentRadius = 6371000; // radius of Earth in meters

            // Calculate the standard gravitational parameter of the parent body
            double gravParameter = gravitationalConstant * parentBodyMass;

            // Calculate the specific angular momentum
            double angularMomentumMagnitude = Math.Sqrt( gravParameter * SemimajorAxis * (1 - Eccentricity * Eccentricity) );
            Vector3Dbl angularMomentum = new Vector3Dbl( angularMomentumMagnitude * Math.Cos( Inclination ) * Math.Sin( ArgumentOfPeriapsis ),
                                                     angularMomentumMagnitude * Math.Sin( Inclination ) * Math.Sin( ArgumentOfPeriapsis ),
                                                     angularMomentumMagnitude * Math.Cos( ArgumentOfPeriapsis ) );

            // Calculate the position and velocity in the orbital plane
            double r = SemimajorAxis * (1 - Eccentricity * Eccentricity) / (1 + Eccentricity * Math.Cos( trueAnomaly ));
            Vector3Dbl positionOrbitalPlane = new Vector3Dbl( r * Math.Cos( trueAnomaly ), r * Math.Sin( trueAnomaly ), 0 );
            Vector3Dbl velocityOrbitalPlane = new Vector3Dbl( -Math.Sqrt( gravParameter / SemimajorAxis ) * Math.Sin( trueAnomaly ), Math.Sqrt( gravParameter / SemimajorAxis ) * (Eccentricity + Math.Cos( trueAnomaly )), 0 );

            // Rotate the position and velocity to the correct orientation in space
            QuaternionDbl rotation1 = QuaternionDbl.AngleAxis( (LongitudeOfAscendingNode - Math.PI / 2), Vector3Dbl.up );
            QuaternionDbl rotation2 = QuaternionDbl.AngleAxis( Inclination, rotation1 * Vector3Dbl.right );
            QuaternionDbl rotation3 = QuaternionDbl.AngleAxis( ArgumentOfPeriapsis, rotation2 * rotation1 * Vector3Dbl.up );
            Vector3Dbl position = rotation3 * rotation2 * rotation1 * positionOrbitalPlane; // + parent position for absolute
            Vector3Dbl velocity = rotation3 * rotation2 * rotation1 * velocityOrbitalPlane;

            return (position, velocity);

            throw new NotImplementedException();
        }*/
    }
}
