using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public interface IConstructionCondition : IPersistent
    {
        /// <summary>
        /// Checks what the build speed multiplier for a constructible at a given position would be.
        /// </summary>
        float GetBuildSpeedMultiplier( Vector3 targetScenePosition );
    }

    public struct CraneBuildCondition : IConstructionCondition
    {
        public float minLiftCapacity;

        public float GetBuildSpeedMultiplier( Vector3 targetScenePosition )
        {
            // get all cranes which range and position overlaps with the scene position.
            throw new NotImplementedException();
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "min_lift_capacity", this.minLiftCapacity }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "min_lift_capacity", out var minLiftCapacity ) )
                this.minLiftCapacity = (float)minLiftCapacity;
        }
    }

    /// <summary>
    /// A data container for an object that can be constructed.
    /// </summary>
    public class FConstructible : MonoBehaviour, IPersistent
    {
        /// <summary>
        /// One build point at 1x build speed takes 1 [s] to build.
        /// </summary>
        [field: SerializeField]
        public float MaxBuildPoints { get; set; }

        [field: SerializeField]
        public List<IConstructionCondition> Conditions { get; set; }

        /// <summary>
        /// Calculates the current build speed multiplier for this specific part, taking into account the build conditions.
        /// </summary>
        public float GetBuildSpeedMultiplier()
        {
            return Conditions.Sum( c => c.GetBuildSpeedMultiplier( this.transform.position ) );
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "max_build_points", this.MaxBuildPoints }
                // todo - conditions.
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "max_build_points", out var maxbuildPoints ) )
                this.MaxBuildPoints = (float)maxbuildPoints;
            // todo - conditions.
        }
    }
}