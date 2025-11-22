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
                return new Color( 0.2f, 0.2f, 0.2f, 0.5f ); // Empty/Dark Grey

            double totalMass = content.GetMass();
            if( totalMass <= 0.000001 )
                return new Color( 0.2f, 0.2f, 0.2f, 0.5f );

            float r = 0, g = 0, b = 0, a = 0;

            // Weighted average of colors
            foreach( (ISubstance s, double mass) in content )
            { 
                float weight = (float)(mass / totalMass);

                Color c = s.DisplayColor;
                r += c.r * weight;
                g += c.g * weight;
                b += c.b * weight;
                a += c.a * weight;
            }

            return new Color( r, g, b, 1.0f ); // Force opaque lines for clarity? Or use 'a' for transparency.
        }
    }
}