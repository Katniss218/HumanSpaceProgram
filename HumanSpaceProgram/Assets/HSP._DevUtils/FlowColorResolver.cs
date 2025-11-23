using UnityEngine;
using HSP.ResourceFlow;


namespace HSP._DevUtils
{
    /// <summary>
    /// Helper to resolve colors from substance collections.
    /// </summary>
    public static class FlowColorResolver
    {
        public static Color GetMixedColor( IReadonlySubstanceStateCollection content )
        {
            if( content == null || content.IsEmpty() )
                return new Color( 0.2f, 0.2f, 0.2f, 1.0f ); // Empty/Dark Grey

            double totalMass = content.GetMass();
            float r = 0, g = 0, b = 0;

            // Mass-weighted average of colors
            foreach( (ISubstance s, double mass) in content )
            { 
                float weight = (float)(mass / totalMass);

                Color color = s.DisplayColor;
                r += color.r * weight;
                g += color.g * weight;
                b += color.b * weight;
            }

            return new Color( r, g, b, 1.0f );
        }
    }
}