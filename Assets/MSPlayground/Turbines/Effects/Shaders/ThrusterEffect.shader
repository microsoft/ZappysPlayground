// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/// <summary>
/// Shader for an animated thruster effect.
/// </summary>
Shader "Playground/ThrusterEffect"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        [Space]

        _WaveAmplitude("Wave Amplitude", Float) = 1
        _WavePeriod("Wave Period", Float) = 1
        _WaveSpeed("Wave Speed", Float) = 1
        _Orientation("Wave Orientation", Vector) = (0,0,1,0)
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha One

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform float4 _Color;

            float _WaveAmplitude;
            float _WavePeriod;
            float _WaveSpeed;
            float4 _Orientation;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                //Calculate position in wave pattern
                float4 modifiedPos = mul(unity_ObjectToWorld, v.vertex);
                float wave = _WaveAmplitude * sin(_WavePeriod * (modifiedPos.z + (_WaveSpeed * (_Time.y))));
                modifiedPos += _Orientation * (1 - v.texcoord.y) * wave;
                o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, modifiedPos));

                //Calculate color
                float4 color = v.color * _Color;
                color.a = v.texcoord.y;
                o.color = color * color.a;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }

}

