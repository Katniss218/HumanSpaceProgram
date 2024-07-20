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

        StateVector GetCurrentStateVector();
        void SetCurrentStateVector( StateVector stateVector );
        StateVector GetStateVectorAtUT( double ut );

        StateVector GetStateVector( float t );

        OrbitalFrame GetCurrentOrbitalFrame();
        OrbitalFrame GetOrbitalFrameAtUT( double ut );
    }
}