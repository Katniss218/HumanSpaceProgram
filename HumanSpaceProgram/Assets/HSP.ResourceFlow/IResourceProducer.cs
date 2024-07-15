using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.Core.ResourceFlowSystem
{
    /// <summary>
    /// Anything that can produce resources from external or internal sources. <br/>
    /// E.g. tank, drilling machine, vacuum cleaner, etc.
    /// </summary>
    public interface IResourceProducer
    {
        Transform transform { get; }

        /// <summary>
        /// Get or set the total outflow per 1 [s].
        /// </summary>
        SubstanceStateCollection Outflow { get; }

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

        /// <summary>
        /// Calculates the amount of resources that would flow out in 1 [s] is an orifice of given area was created at the specified position.
        /// </summary>
        /// <param name="opposingPressure"></param>
        /// <returns>A new <see cref="SubstanceStateCollection"/> object containing the resources that can flow out in 1 [s].</returns>
        (SubstanceStateCollection, FluidState) SampleFlow( Vector3 localPosition, Vector3 localAcceleration, float holeArea, float dt, FluidState opposingFluid );
    }
}