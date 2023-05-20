using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatnisssSpaceSimulator.Core.ResourceFlowSystem
{
    /// <summary>
    /// Represents an object that can hold resources (substances).
    /// </summary>
    public interface IResourceContainer
    {
        /// <summary>
        /// The maximum volumetric capacity of this container.
        /// </summary>
        float MaxVolume { get; }

        /// <summary>
        /// The current contents of this container.
        /// </summary>
        SubstanceStateCollection Contents { get; }
    }
}