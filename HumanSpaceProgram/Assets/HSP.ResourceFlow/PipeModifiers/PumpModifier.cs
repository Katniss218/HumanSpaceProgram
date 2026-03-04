using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    public class PumpModifier : IPipeModifier
    {
        public double HeadAdded { get; set; } = 0.0;

        public void Apply( FlowPipe pipe )
        {
            // A pump adds potential head to the flow calculation.
            pipe.HeadAdded += HeadAdded;
        }

        [MapsInheritingFrom( typeof( PumpModifier ) )]
        public static IDescriptor PumpModifierMapping()
        {
            return new MemberwiseDescriptor<PumpModifier>()
                .WithMember( "head_added", o => o.HeadAdded );
        }
    }
}