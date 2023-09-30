using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// Specifies that a component has mass.
    /// </summary>
    public interface IHasMass : IComponent
    {
        public delegate void MassChange( float massDelta );

        /// <summary>
        /// The total mass of the component.
        /// </summary>
        float Mass { get; }

        event MassChange OnAfterMassChanged;
    }
}