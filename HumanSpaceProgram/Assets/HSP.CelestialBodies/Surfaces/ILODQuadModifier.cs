using System.Collections.Generic;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// Represents an arbitrary LOD sphere modifier.
    /// </summary>
    /// <remarks>
    /// Modifiers are used to change the data of the quad mesh during generation.
    /// </remarks>
    public interface ILODQuadModifier
    {
        /// <summary>
        /// Determines which LOD sphere modes the job should be executed for.
        /// </summary>
        public LODQuadMode QuadMode { get; }

        /// <summary>
        /// Gets the job that can execute the work of this modifier.
        /// </summary>
        public ILODQuadJob GetJob();

        /// <summary>
        /// Filters modifiers, returning only the ones used in the particular build mode.
        /// </summary>
        /// <param name="modifiersInStages">The collection of modifiers, split up into stages - modifiersInStages[stage][modifier]</param>
        /// <returns>An array of all jobs matching the specified build mode, and an array containing the indices of the first job from each subsequent stage.</returns>
        public static (ILODQuadModifier[] modifiers, int[] firstModifierPerStage) FilterJobs( ILODQuadModifier[][] modifiersInStages, LODQuadMode buildMode )
        {
            List<ILODQuadModifier> filteredModifiers = new();
            List<int> firstModifierPerStage = new();

            foreach( var stage in modifiersInStages )
            {
                int stageStart = filteredModifiers.Count;
                bool anythingInStageAdded = false;

                foreach( var modifier in stage )
                {
                    // All jobs that intersect the desired build mode.
                    if( ((int)modifier.QuadMode & (int)buildMode) != 0 )
                    {
                        filteredModifiers.Add( modifier );
                        anythingInStageAdded = true;
                    }
                }
                if( anythingInStageAdded )
                {
                    firstModifierPerStage.Add( stageStart );
                }
            }

            return (filteredModifiers.ToArray(), firstModifierPerStage.ToArray());
        }
    }
}