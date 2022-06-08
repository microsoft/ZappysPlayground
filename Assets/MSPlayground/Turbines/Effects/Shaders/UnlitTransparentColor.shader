// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/// <summary>
/// Basic unlit transparent color shader with no texture.
/// </summary>

Shader "Unlit/Transparent Color"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Color[_Color]
        }
    }
}
