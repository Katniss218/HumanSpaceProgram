using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Control.Controls
{
    public abstract class ControlParameterOutput : Control { }

    /// <summary>
    /// Represents a control that can directly get a specific parameter. <br/>
    /// Canonically, the parameter should relate to the same 'object' the control is located on.
    /// </summary>
    public sealed class ControlParameterOutput<T> : ControlParameterOutput
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

        public override IEnumerable<Control> GetConnections()
        {
            yield return Input;
        }

        public override bool TryConnect( Control other )
        {
            if( other is not ControlParameterInput<T> input )
                return false;

            ControlParameterInput<T>.Connect( input, this );
            return true;
        }

        public override bool TryDisconnect( Control other )
        {
            if( other is not ControlParameterInput<T> input )
                return false;

            if( this.Input != input )
                return false;

            ControlParameterInput<T>.Disconnect( input, this );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.Input == null )
                return false;

            ControlParameterInput<T>.Disconnect( this.Input, this );
            return true;
        }
    }
}