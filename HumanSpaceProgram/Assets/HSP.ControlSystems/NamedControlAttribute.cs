using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Control
{
    /// <summary>
    /// Marks a field as a 'control' (an endpoint of the control system). <br/>
    /// The control will have a specific name next to it, and will show a specified description when hovered over. <br/>
    /// <br/>
    /// Only works on fields of type derived from <see cref="Control"/> or <see cref="ControlGroup"/>, or their corresponding array/List{T} types.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field )]
    public sealed class NamedControlAttribute : Attribute
    {
        /// <summary>
        /// The name to display to the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description to display to the player.
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// If true, the control will not be able to be edited by the player in the control setup window.
        /// </summary>
        public bool Editable { get; set; } = true;

        public NamedControlAttribute( string name )
        {
            this.Name = name;
        }

        public NamedControlAttribute( string name, string description )
        {
            this.Name = name;
            this.Description = description;
        }
    }
}