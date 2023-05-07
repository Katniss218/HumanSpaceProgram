using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// Represents a generalized concept of persistent modular objects that exist in the game world.
    /// </summary>
    public abstract class Functionality : MonoBehaviour
    {
        /// <summary>
        /// Save persistent data (e.g. to a save file).
        /// </summary>
        public abstract JToken Save();

        /// <summary>
        /// Load persistent data (e.g. from a save file).
        /// </summary>
        public abstract void Load( JToken data );

        // -------------------------------------------------------------------------------
        //      NOTES TO IMPLEMENTERS:

        // Functionalities don't necessarily have to go on vessels/parts.

        // The game is paused if `Time.timeScale == 0`.

    }
}