using KatnisssSpaceSimulator.Core;
using Newtonsoft.Json.Linq;
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
        public struct Resource
        {
            public string ID { get; set; }
            public float Volume { get; set; }
        }

        /// <summary>
        /// The total available volume of the container, in [m^3].
        /// </summary>
        [field: SerializeField]
        public float MaxVolume { get; private set; }

        /// <summary>
        /// Resource currently contained in a container.
        /// </summary>
        [field: SerializeField]
        public Resource[] Resources { get; set; }

        [field: SerializeField]
        public FBulkContainer[] ConnectedTo { get; set; } // a list of one-way connections mayhaps?

        // flow between containers depends on many factors.
        // - acceleration on the container (ullage, only for liquids)
        // - pressure difference across the containers (both liquids and gasses).
        // - for mixed liquid + gas, gas will tend to accumulate on the opposite side to the liquid.
        // - for mixed liquid the densest will accumulate at the bottom.
        // - for mixed solid (gravel-like), orientation depends on where and when the resources came from. They don't tend to separate into layers based on density, unlike liquids and gasses.

        // - ullage purposes assume spherical tank??? maybe?
        //   - the orientation of the inlet could determine where on the sphere it is.

        // tanks could have an arbitrary number of connections depending on where the player puts the pipes?

        // the connections would be set up in the VAB / by the save loader.

        public override JToken Save()
        {
            throw new NotImplementedException();

            return new JObject()
            {
                { "Resources", this.Resources.ToString() }, // temp
                { "ConnectedTo", this.ConnectedTo.ToString() } // temp
            };
        }

        public override void Load( JToken data )
        {
            throw new NotImplementedException();

            this.Resources = new Resource[] { }; // temp
            this.ConnectedTo = new FBulkContainer[] { }; // temp
        }
    }
}