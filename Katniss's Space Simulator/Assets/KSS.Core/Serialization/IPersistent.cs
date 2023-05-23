using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Describes a component that should be saved when saving the game.
    /// </summary>
    public interface IPersistent
    {
        /// <summary>
        /// Saves the object's persistent data to a JSON object.
        /// </summary>
        /// <param name="fileVersion">The version number of the data file. Used for backwards compatibility.</param>
        JToken Save( int fileVersion );

        /// <summary>
        /// Loads the object's persistent data from a JSON object.
        /// </summary>
        /// <param name="fileVersion">The version number of the data file. Used for backwards compatibility.</param>
        void Load( int fileVersion, JToken data );
    }
}