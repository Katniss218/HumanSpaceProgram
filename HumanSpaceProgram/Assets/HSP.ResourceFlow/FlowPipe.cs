using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public enum FlowPipeType { Passive, Pump, Valve }
    public class FlowPipe
    {
        IResourceProducer _end1P;
        IResourceConsumer _end1C;

        IResourceProducer _end2P;
        IResourceConsumer _end2C;

        public float CrossSectionArea = 0.1f; // m^2
        public float ValveOpenFraction = 1f; // 0-1
        public FlowPipeType Type = FlowPipeType.Passive;
        public float PumpDeltaP = 0f; // Pa for pump

        public FlowPipe( FlowInlet from, FlowInlet to )
        {
            throw new NotImplementedException();
        }

        // compute volumetric flow rate (m^3/s) using simplified Torricelli-like relation
        // Q = A * v; v = sqrt(2 * deltaP / rho)
        // we cap and handle negative deltaP
        public float ComputeFlowRate()
        {
            // get pressures at the inlet nodes. If null, use 0
            var pFrom = FromTank?.GetPressureAtOutlet() ?? 0f;
            var pTo = ToTank?.GetPressureAtInlet() ?? 0f;
            var deltaP = pFrom - pTo + PumpDeltaP;
            // estimate density
            var rho = (FromTank?.GetEffectiveDensityAtOutlet() ?? 1000f);
            if( rho <= 0f ) rho = 1f;
            if( deltaP <= 0f ) return 0f; // no backflow in passive pipe for Phase1
            var velocity = Mathf.Sqrt( 2f * deltaP / rho );
            var q = (CrossSectionArea * ValveOpenFraction) * velocity;
            return q;
        }
    }
}