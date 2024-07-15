using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.Core.ResourceFlowSystem
{
    /// <summary>
    /// Anything that can consume resources. E.g. rocket engine, lightbulb (elec), vent, etc.
    /// </summary>
    public interface IResourceConsumer : IComponent
    {
        /// <summary>
        /// Get or set the total inflow per 1 [s].
        /// </summary>
        SubstanceStateCollection Inflow { get; }

        /// <summary>
        /// Clamps the flow based on how much fluid can actually flow into the consumer object in 1 [s].
        /// </summary>
        void ClampIn( SubstanceStateCollection inflow, float dt );

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container, as well as what species will want to `flow` out of the container.
        /// </summary>
        /// <remarks>
        /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="localAcceleration">The local acceleration vector, in [m/s^2].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea );
    }
}