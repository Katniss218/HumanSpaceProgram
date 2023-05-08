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
    /// A container for a <see cref="BulkResource"/>.
    /// </summary>
    public class FBulkContainer : Functionality
    {
        /// <summary>
        /// The total available volume of the container, in [m^3].
        /// </summary>
        [field: SerializeField]
        public float MaxVolume { get; private set; }

        /// <summary>
        /// Determines the center position of the container.
        /// </summary>
        [field: SerializeField]
        public Transform VolumeTransform { get; set; }

        [field: SerializeField]
        public float Volume { get; set; }

        /// <summary>
        /// Inflow minus outflow. positive = inflow, negative = outflow.
        /// </summary>
        [field: SerializeField]
        internal float TotalFlow { get; set; }

        void FixedUpdate()
        {
            Volume += TotalFlow * Time.fixedDeltaTime;
            // TODO - update the mass too, because the fluid weighs something.
        }

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

            /*return new JObject()
            {
                { "Resources", this.Resources.ToString() }, // temp
                { "ConnectedTo", this.ConnectedTo.ToString() } // temp
            };*/
        }

        public override void Load( JToken data )
        {
            throw new NotImplementedException();

            //this.Resources = new Resource[] { }; // temp
            //this.ConnectedTo = new FBulkContainer[] { }; // temp
        }
    }
}