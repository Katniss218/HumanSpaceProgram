using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Core
{

    /// <summary>
    /// Inherit from this class to provide an implementation of a hierarchy instantiator, and a metadata provider.
    /// </summary>
    public abstract class PartFactory
    {
        /// <summary>
        /// Loads the part metadata of this specific part from the original source.
        /// </summary>
        /// <returns>The loaded part metadata.</returns>
        public abstract PartMetadata LoadMetadata();

        /// <summary>
        /// Instantiates the gameobject hierarchy of this specific part from the original source.
        /// </summary>
        /// <returns>The root game object of the instantiated hierarchy.</returns>
        public abstract GameObject Load( IForwardReferenceMap refMap );
    }
}
