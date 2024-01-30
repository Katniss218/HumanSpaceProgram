using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Control
{
    /// <summary>
    /// Marks a field to be managed by the control setup system. <br/>
    /// The control will have a specific name next to it, and will show a specified description when hovered over. <br/>
    /// <br/>
    /// Only works on fields of type derived from <see cref="Control"/> or <see cref="ControlGroup"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field )]
    public sealed class NamedControlAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; } = null;

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