using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core
{
    /// <summary>
    /// Specifies a component that can have its mass changed by other components.
    /// </summary>
    public interface IHasMass
    {
        /// <summary>
        /// The total mass of the object.
        /// </summary>
        public float Mass { get; set; }
    }
}