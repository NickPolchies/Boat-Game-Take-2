Shader "Gradient Noise"
{
Properties
{
_RippleSpeed("RippleSpeed", Float) = 1
_BaseColor("BaseColor", Color) = (0, 0.8745098, 1, 0)
_RippleDensity("RippleDensity", Float) = 70
_Slimness("Slimness", Float) = 5
[HDR]_RippleColor("RippleColor", Color) = (0.2588235, 1.647059, 2, 0)
_WaveSpeed("WaveSpeed", Float) = 0.1
_WaveLength("WaveLength", Float) = 5
_WaveAmplitude("WaveAmplitude", Float) = 0.5
}
SubShader
{
Tags
{
// RenderPipeline: <None>
"RenderType"="Opaque"
"Queue"="Geometry"
"ShaderGraphShader"="true"
}
Pass
{
    // Name: <None>
    Tags
    {
        // LightMode: <None>
    }

    // Render State
    // RenderState: <None>

    // Debug
    // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

    // Pragmas
    #pragma vertex vert
#pragma fragment frag

    // DotsInstancingOptions: <None>
    // HybridV1InjectedBuiltinProperties: <None>

    // Keywords
    // PassKeywords: <None>
    // GraphKeywords: <None>

    // Defines
    #define ATTRIBUTES_NEED_TEXCOORD0
    #define VARYINGS_NEED_TEXCOORD0
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
    #define SHADERPASS SHADERPASS_PREVIEW
#define SHADERGRAPH_PREVIEW 1
    /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

    // Includes
    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

    struct Attributes
{
 float3 positionOS : POSITION;
 float4 uv0 : TEXCOORD0;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
 float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
 FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
 float4 uv0;
 float3 TimeParameters;
};
struct VertexDescriptionInputs
{
};
struct PackedVaryings
{
 float4 positionCS : SV_POSITION;
 float4 interp0 : INTERP0;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
 FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};

    PackedVaryings PackVaryings (Varyings input)
{
PackedVaryings output;
ZERO_INITIALIZE(PackedVaryings, output);
output.positionCS = input.positionCS;
output.interp0.xyzw =  input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
output.instanceID = input.instanceID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
output.cullFace = input.cullFace;
#endif
return output;
}

Varyings UnpackVaryings (PackedVaryings input)
{
Varyings output;
output.positionCS = input.positionCS;
output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
output.instanceID = input.instanceID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
output.cullFace = input.cullFace;
#endif
return output;
}


    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float _RippleSpeed;
float4 _BaseColor;
float _RippleDensity;
float _Slimness;
float4 _RippleColor;
float _WaveSpeed;
float _WaveLength;
float _WaveAmplitude;
CBUFFER_END

// Object and Global properties

    // Graph Includes
    // GraphIncludes: <None>

    // -- Property used by ScenePickingPass
    #ifdef SCENEPICKINGPASS
    float4 _SelectionID;
    #endif

    // -- Properties used by SceneSelectionPass
    #ifdef SCENESELECTIONPASS
    int _ObjectId;
    int _PassValue;
    #endif

    // Graph Functions
    
void Unity_Multiply_float_float(float A, float B, out float Out)
{
Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}


float2 Unity_GradientNoise_Dir_float(float2 p)
{
    // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
    p = p % 289;
    // need full precision, otherwise half overflows when p > 1
    float x = float(34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
{
    float2 p = UV * Scale;
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
    float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
}

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

    // Graph Vertex
    // GraphVertex: <None>

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreSurface' */

    // Graph Pixel
    struct SurfaceDescription
{
float4 Out;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float _Property_4fea02d4582c4e02a0fd215e448cda69_Out_0 = _WaveSpeed;
float _Multiply_1d2b133be5f24f4b88c962cf5978e259_Out_2;
Unity_Multiply_float_float(IN.TimeParameters.x, _Property_4fea02d4582c4e02a0fd215e448cda69_Out_0, _Multiply_1d2b133be5f24f4b88c962cf5978e259_Out_2);
float2 _TilingAndOffset_df9ad2951b724440a18c793121f36cee_Out_3;
Unity_TilingAndOffset_float(IN.uv0.xy, float2 (10, 10), (_Multiply_1d2b133be5f24f4b88c962cf5978e259_Out_2.xx), _TilingAndOffset_df9ad2951b724440a18c793121f36cee_Out_3);
float _Property_ffacadf3b626446286d8b9d8fd161f0e_Out_0 = _WaveLength;
float _GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2;
Unity_GradientNoise_float(_TilingAndOffset_df9ad2951b724440a18c793121f36cee_Out_3, _Property_ffacadf3b626446286d8b9d8fd161f0e_Out_0, _GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2);
surface.Out = all(isfinite(_GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2)) ? half4(_GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2, _GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2, _GradientNoise_b001eb54b245467aac8b28752ec2d73f_Out_2, 1.0) : float4(1.0f, 0.0f, 1.0f, 1.0f);
return surface;
}

    // --------------------------------------------------
    // Build Graph Inputs

    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */





    output.uv0 =                                        input.texCoord0;
    output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/PreviewVaryings.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/PreviewPass.hlsl"

    ENDHLSL
}
}
CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
FallBack "Hidden/Shader Graph/FallbackError"
}