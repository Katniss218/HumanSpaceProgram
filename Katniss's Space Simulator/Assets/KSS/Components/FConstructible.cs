using KSS.Core;
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
        public enum State
        {
            FinishedDeconstruction = -1,
            InProgress = 0,
            FinishedConstruction = 1
        }

        public float BuildPoints { get; private set; }

        public void ChangeBuildPoints( float delta )
        {
            this.BuildPoints += delta;
            OnAfterBuildPointsChanged( delta );
        }

        /// <summary>
        /// The total number of build points required to complete the colstruction/deconstruction of this part.
        /// </summary>
        /// <remarks>
        /// One build point at 1x build speed takes 1 [s] to build.
        /// </remarks>
        [field: SerializeField]
        public float MaxBuildPoints { get; set; }

        public float BuildPercent => BuildPoints / MaxBuildPoints;

#warning TODO - maybe store the current number of build points here, and handle the part being complete/incomplete here? it would also work as sort of "hit points" of a part.

        public State CurrentState { get; set; } // state can be more than just ghost. A ghost would be for things that are not built yet. there can be a separate state for in-progress, etc.

        void OnAfterBuildPointsChanged( float delta )
        {
            if( BuildPercent <= 0.0f || BuildPercent >= 1.0f )
                this.transform.GetVessel().RecalculateParts();

            // the main "problem" here is that we need to store the original materials/shaders of the parts that were spawned.
        }

        void OnEnable()
        {
            // store original materials, replace original with new.
            // originals will be saved here to persist the state.
        }
        
        void OnDisable()
        {
            // stop tracking materials, restore original materials.
        }

        // patches can be applied on the fly because the objects are already in memory, and we know what we want to do to them.

        // construction site would only manage the distribution of build points, etc.

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