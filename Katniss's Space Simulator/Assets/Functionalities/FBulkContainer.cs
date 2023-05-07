using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities
{
    /// <summary>
    /// A container for <see cref="BulkResource"/>.
    /// </summary>
    public class FBulkContainer : Functionality
    {
        [field: SerializeField]
        public float MaxVolume { get; private set; }

        /// <summary>
        /// Resource currently contained in a container.
        /// </summary>
        public BulkResource Resource { get; set; }

        /// <summary>
        /// Amount of resource currently contained in a container.
        /// </summary>
        public float Volume { get; set; }

        // flow between containers depends on many factors.
        // - acceleration on the container (ullage, only for liquids)
        // - pressure difference across the containers (both liquids and gasses).
        // - for mixed liquid + gas, gas will tend to accumulate on the opposite side to the liquid.
        // - for mixed liquid the densest will accumulate at the bottom.
        // - for mixed solid (gravel-like), orientation depends on where and when the resources came from. They don't tend to separate into layers based on density, unlike liquids and gasses.
    }
}