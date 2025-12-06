using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    public class ValveModifier : IPipeModifier
    {
        public double PercentOpen { get; set; } = 1.0;

        public void Apply( FlowPipe pipe )
        {
            // A valve's effect is to scale the pipe's base conductance.
            pipe.Conductance *= PercentOpen;
        }

        [MapsInheritingFrom( typeof( ValveModifier ) )]
        public static SerializationMapping ValveModifierMapping()
        {
            return new MemberwiseSerializationMapping<ValveModifier>()
                .WithMember( "percent_open", o => o.PercentOpen );
        }
    }
}