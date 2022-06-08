// For info on stereo rendering in custom shaders,
// https://docs.unity3d.com/Manual/SinglePassInstancing.html
//

Shader "Playground/Flipbook"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0,1,0,0)
		_Columns("Columns", int) = 4
		_Rows("Rows", int) = 2
		_Frame("Current frame", int) = 0
		_SizeX("Size X", float) = 1
		_SizeY("Size Y", float) = 1
		_OffsetX("Offset X", Range(-0.5, 0.5)) = 0
		_OffsetY("Offset Y", Range(-0.5, 0.5)) = 0
	}

	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert 
			#pragma fragment Frag
			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 posOS : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 posWS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 frameStart : TEXCOORD1;
				float2 frameEnd : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			half4 _Color;
			float _Columns;
			float _Rows;
			float _Frame;
			float _SizeX;
			float _SizeY;
			float _OffsetX;
			float _OffsetY;

			Varyings Vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_OUTPUT(Varyings, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.posWS = UnityObjectToClipPos(input.posOS);

				//Calculate the uv coordinates for the current frame of the flipbook
				float2 frameSize = float2(1.0f / _Columns, 1.0f / _Rows);
				float totalFrames = _Columns * _Rows;
				float index = ceil(_Frame);
				float indexX = index % _Columns;
				float indexY = floor((index % totalFrames) / _Columns);
				float2 size = 1 - (float2(_SizeX, _SizeY) - 2);

				float2 offset = float2(frameSize.x * indexX, -frameSize.y * indexY) - (0.5f * frameSize * (size - 1));
				float2 frameUV = input.uv * frameSize * size;
				frameUV.y = frameUV.y + frameSize.y * (_Rows - 1);

				//Set UV position and texture cutoff positions
				output.uv = frameUV + offset - float2(_OffsetX, _OffsetY);
				output.frameStart = output.uv - float2(frameSize.x * indexX, -frameSize.y * (indexY - 1));
				output.frameEnd = output.frameStart + float2(1 - frameSize.x, frameSize.y);

				return output;
			}

			half4 Frag(Varyings input) : SV_TARGET
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				half4 col = tex2D(_MainTex, input.uv);

				//Clip pixels outside of desired frame
				clip(input.frameStart);
				clip(1 - input.frameEnd);

				return col * _Color * col.a * _Color.a;
			}
			ENDCG
		}
	}
}