using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    public class ValveModifier : IPipeModifier
    {
        public double PercentOpen { get; set; } = 1.0;

        public void Apply( FlowPipe pipe )
        {
#warning TODO - this is brittle and non-reversible.
            // A valve's effect is to scale the pipe's base conductance.
            pipe.MassFlowConductance *= PercentOpen;
        }

        [MapsInheritingFrom( typeof( ValveModifier ) )]
        public static SerializationMapping ValveModifierMapping()
        {
            return new MemberwiseSerializationMapping<ValveModifier>()
                .WithMember( "percent_open", o => o.PercentOpen );
        }
    }
}