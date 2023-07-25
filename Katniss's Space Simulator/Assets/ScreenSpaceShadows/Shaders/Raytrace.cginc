// Experimental implementation of contact shadows for Unity
// https://github.com/keijiro/ContactShadows

#include "Common.cginc"

// Shadow mask texture
sampler2D _ShadowMask;

// Noise texture (used for dithering)
sampler2D _NoiseTex;
float2 _NoiseScale;

// Light vector, but it has more calculations done to it on the C# side, it's not just the direction, it also has non-one length.
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
        return mask; // if already in shadow, return that.

    // Temporally distributed noise offset
    //float offs = tex2D(_NoiseTex, uv * _NoiseScale).a;
    //offs = 0; // temporary for simplification
    
    // View space position of the origin
    float depth = SampleDepth(uv);
    if (depth > 0.999999) 
        return mask; // BG early-out
    
    float3 fragPosViewSpace = InverseProjectUVZ(uv, depth);
    
    float3 step = (_LightVector * 1) / _SampleCount;
    
    // raymarch from the view space position of the pixel towards the light
    // check if the depth value at that pixel if "in front".
    // More samples = more steps.
    UNITY_LOOP for (uint i = 0; i < _SampleCount; i++)
    {
        // Position of the current sample
        float3 currentPosViewSpace = fragPosViewSpace + (step * (i /*+ offs * 2*/)) + step;
        
        // View space position of the depth sample
        float3 vp_depth = InverseProjectUV(ProjectVP(currentPosViewSpace)); // InverseProjectUV samples the depth buffer.

        // todo - this could be marched in clip space, and reduce the number of samples..
        
        // Depth difference between ray/depth sample
        // Negative: Ray sample is closer to the camera (not occluded)
        // Positive: Ray sample is beyond the depth sample (possibly occluded)
        float diff = currentPosViewSpace.z - vp_depth.z;

        // Occlusion test - depth is positive and within the max threshold.
        if (diff > 0.05 /** (1 - offs)*/ && diff < _MaxDepthDifference)
            return 0;
    }

    return mask;
}
