using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Construction
{

    /// <summary>
    /// Specifies the state of (de)construction.
    /// </summary>
    public enum ConstructionState : sbyte
    {
        /// <summary>
        /// Indicates that the (de)construction is waiting for the player to approve (start) it.
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Construction is ongoing.
        /// </summary>
        Constructing = 1,
        /// <summary>
        /// Construction is paused.
        /// </summary>
        PausedConstructing = 2,
        /// <summary>
        /// Deconstruction is ongoing.
        /// </summary>
        Deconstructing = -1,
        /// <summary>
        /// Deconstruction is paused.
        /// </summary>
        PausedDeconstructing = -2
    }

    public static class ConstructionState_Ex
    {
        /// <summary>
        /// Does the state represents construction (either ongoing or paused)?
        /// </summary>
        public static bool IsConstruction( this ConstructionState state )
        {
            return (int)state > 0;
        }

        /// <summary>
        /// Does the state represents deconstruction (either ongoing or paused)?
        /// </summary>
        public static bool IsDeconstruction( this ConstructionState state )
        {
            return (int)state < 0;
        }

        /// <summary>
        /// Does the state represents deconstruction (either ongoing or paused)?
        /// </summary>
        public static bool IsPaused( this ConstructionState state )
        {
            if( state == ConstructionState.NotStarted )
                return false;

            return ((int)state % 2) == 0;
        }

        /// <summary>
        /// Does the state represents deconstruction (either ongoing or paused)?
        /// </summary>
        public static bool IsInProgress( this ConstructionState state )
        {
            if( state == ConstructionState.NotStarted )
                return false;

            return ((int)state % 2) == 1;
        }
    }
}