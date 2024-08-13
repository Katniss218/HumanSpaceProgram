using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /*public struct Continuous<T>
    {
        public T value;
        public Func<double, T> valueGetter;

        public static implicit operator Continuous<T>( T value )
        {
            return new Continuous<T>() { value = value };
        }

        public static implicit operator Continuous<T>( Func<double, T> valueGetter )
        {
            return new Continuous<T>() { valueGetter = valueGetter };
        }
    }*/
    
    public interface ITrajectory
    {
        void AddAcceleration( Vector3Dbl acceleration );
        void AddAccelerationAtUT( Vector3Dbl acceleration, double ut );

        OrbitalStateVector GetCurrentStateVector();
        void SetCurrentStateVector( OrbitalStateVector stateVector );
        OrbitalStateVector GetStateVectorAtUT( double ut );

        OrbitalStateVector GetStateVector( float t );

        OrbitalFrame GetCurrentOrbitalFrame();
        OrbitalFrame GetOrbitalFrameAtUT( double ut );
    }
}