using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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
        public static IDescriptor ValveModifierMapping()
        {
            return new MemberwiseDescriptor<ValveModifier>()
                .WithMember( "percent_open", o => o.PercentOpen );
        }
    }
}