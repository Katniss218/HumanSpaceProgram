using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Control.Controls
{
    /// <summary>
    /// Represents a control that can directly get a specific parameter. <br/>
    /// Canonically, the parameter should relate to the same 'object' the control is located on.
    /// </summary>
    public sealed class ControlParameterOutput<T> : Control
    {
        internal Func<T> getter;

        public ControlParameterInput<T> Input { get; internal set; }

        /// <param name="getter">Returns the value of the parameter.</param>
        public ControlParameterOutput( Func<T> getter )
        {
            if( getter == null )
                throw new ArgumentNullException( nameof( getter ), $"The parameter getter must be initialized to a non-null value." );

            this.getter = getter;
        }

        public override void Disconnect()
        {
            ControlParameterInput<T>.Disconnect( this.Input, this );
        }
    }
}