// Experimental implementation of contact shadows for Unity
// https://github.com/keijiro/ContactShadows

#include "Common.cginc"

// Shadow mask texture
sampler2D _ShadowMask;

// Noise texture (used for dithering)
sampler2D _NoiseTex;
float2 _NoiseScale;

// Light vector
// (reversed light direction in view space) * (ray-trace sample interval)
float3 _LightVector;

// How far in front of the ray position can the depth of an occluding pixel be
float _MaxDepthDifference;

// Total sample count
uint _SampleCount;

// Fragment shader - Screen space ray-trancing shadow pass
half4 FragmentShadow(float2 uv : TEXCOORD) : SV_Target
{
    float mask = tex2D(_ShadowMask, uv).r;
    if (mask < 0.01) 
        return mask;

    // Temporal distributed noise offset
    float offs = tex2D(_NoiseTex, uv * _NoiseScale).a;
    offs = 0; // temporary for simplification
    
    // View space position of the origin
    float depth = SampleDepth(uv);
    if (depth > 0.999999) 
        return mask; // BG early-out
    
    float3 fragPosViewSpace = InverseProjectUVZ(uv, depth);

    // raymarch from the view space position of the pixel towards the light
    // check if the depth value at that pixel if "in front".
    // More samples = more steps.
    UNITY_LOOP for (uint i = 0; i < _SampleCount; i++)
    {
        // this needs another safeguard or two.
        
        // Position of the current sample
        float3 currentPosViewSpace = fragPosViewSpace + _LightVector * (i + offs * 2);
        
        // View space position of the depth sample
        float3 vp_depth = InverseProjectUV(ProjectVP(currentPosViewSpace)); // InverseProjectUV samples the depth buffer.

        // Depth difference between ray/depth sample
        // Negative: Ray sample is closer to the camera (not occluded)
        // Positive: Ray sample is beyond the depth sample (possibly occluded)
        float diff = currentPosViewSpace.z - vp_depth.z;

        // Occlusion test - depth is positive and within the max threshold.
        if (diff > 0.01 * (1 - offs) && diff < _MaxDepthDifference)
            return 0;
    }

    return mask;
}
