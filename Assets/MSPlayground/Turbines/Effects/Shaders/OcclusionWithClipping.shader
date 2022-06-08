// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/// <summary>
/// Occlusion shader that works with Clipping Primitives.
/// </summary>
Shader "Playground/Occlusion With Clipping"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-1" }

        ZWrite On
        ZTest LEqual
        ColorMask 0

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CLIPPING_PLANE _CLIPPING_SPHERE _CLIPPING_BOX

            #include "UnityCG.cginc"

#if defined(_CLIPPING_PLANE) || defined(_CLIPPING_SPHERE) || defined(_CLIPPING_BOX)
            #define _CLIPPING_PRIMITIVE
#else
            #undef _CLIPPING_PRIMITIVE
#endif
            /// <summary>
            /// Point in primitive methods. (From GraphicsToolsCommon in MRTK Graphics Tools)
            /// </summary>

            inline float PointVsPlane(float3 worldPosition, float4 plane)
            {
                float3 planePosition = plane.xyz * plane.w;
                return dot(worldPosition - planePosition, plane.xyz);
            }

            inline float PointVsSphere(float3 worldPosition, float4x4 sphereInverseTransform)
            {
                return length(mul(sphereInverseTransform, float4(worldPosition, 1.0)).xyz) - 0.5;
            }

            inline float PointVsBox(float3 worldPosition, float4x4 boxInverseTransform)
            {
                float3 distance = abs(mul(boxInverseTransform, float4(worldPosition, 1.0)).xyz) - 0.5;
                return length(max(distance, 0.0)) + min(max(distance.x, max(distance.y, distance.z)), 0.0);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 worldPosition : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //From GraphicsToolsCommon in MRTK Graphics Tools
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

#if defined(_CLIPPING_PLANE)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _ClipPlaneSide)
            UNITY_DEFINE_INSTANCED_PROP(float4, _ClipPlane)
#endif

#if defined(_CLIPPING_SPHERE)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _ClipSphereSide)
            UNITY_DEFINE_INSTANCED_PROP(float4x4, _ClipSphereInverseTransform)
#endif

#if defined(_CLIPPING_BOX)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _ClipBoxSide)
            UNITY_DEFINE_INSTANCED_PROP(float4x4, _ClipBoxInverseTransform)
#endif

            UNITY_INSTANCING_BUFFER_END(Props)


            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_OUTPUT(v2f, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.position = UnityObjectToClipPos(input.vertex);
                output.worldPosition.xyz = mul(unity_ObjectToWorld, input.vertex).xyz;

                return output;
            }


            fixed4 frag(v2f input) : SV_Target
            {
                //Primitive clipping from GraphicsToolsCommon in MRTK Graphics Tools
#if defined(_CLIPPING_PRIMITIVE)
                float primitiveDistance = 1.0;
#if defined(_CLIPPING_PLANE)
                fixed clipPlaneSide = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipPlaneSide);
                float4 clipPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipPlane);
                primitiveDistance = min(primitiveDistance, PointVsPlane(input.worldPosition.xyz, clipPlane) * clipPlaneSide);
#endif
#if defined(_CLIPPING_SPHERE)
                fixed clipSphereSide = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipSphereSide);
                float4x4 clipSphereInverseTransform = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipSphereInverseTransform);
                primitiveDistance = min(primitiveDistance, PointVsSphere(input.worldPosition.xyz, clipSphereInverseTransform) * clipSphereSide);
#endif
#if defined(_CLIPPING_BOX)
                fixed clipBoxSide = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipBoxSide);
                float4x4 clipBoxInverseTransform = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipBoxInverseTransform);
                primitiveDistance = min(primitiveDistance, PointVsBox(input.worldPosition.xyz, clipBoxInverseTransform) * clipBoxSide);
#endif
#endif
                //Clip or output black (occlusion on AR)
#if defined(_CLIPPING_PRIMITIVE)
                clip(primitiveDistance);
#endif
                return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
