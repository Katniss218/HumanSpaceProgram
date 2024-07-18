using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP
{
    /// <summary>
    /// Specifies that a component has mass.
    /// </summary>
    public interface IHasMass : IComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="massDelta">The difference between the old mass and the new mass, in [kg]. Positive if the mass was increased, negative otherwise.</param>
        public delegate void MassChange( float massDelta );

        /// <summary>
        /// The mass of the component, in [kg].
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// Invoked after the mass of the component is changed.
        /// </summary>
        event MassChange OnAfterMassChanged;
    }
}