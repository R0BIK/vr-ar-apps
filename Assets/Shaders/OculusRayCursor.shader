Shader "Interaction/OculusRayCursor"
{
    Properties
    {
        _Radius("Radius", FLOAT) = 0.5
        _OutlineThickness("Outline Thickness", FLOAT) = 0.05
        _Color("Inner Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0.5,0.5,0.5,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZTest LEqual
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -5, -5

        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Radius;
                float _OutlineThickness;
                float4 _Color;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float dist = distance(input.uv, float2(0.5, 0.5));
                float2 ddDist = float2(ddx(dist), ddy(dist));
                float ddDistLen = rcp(length(ddDist));

                float4 innerColor = _Color;
                float4 borderColor = _OutlineColor;

                float outerRadius = _Radius;
                float innerRadius = _OutlineThickness - _Radius;

                float outerDist = dist - outerRadius;
                float outerDistOverLen = outerDist * ddDistLen;

                float innerDist = dist + innerRadius;
                float innerDistOverLen = innerDist * ddDistLen;
                
                float colorLerpParam = saturate(innerDistOverLen);
                float4 fragColor = lerp(innerColor, borderColor, colorLerpParam);
                fragColor.a *= (1.0 - saturate(outerDistOverLen));
                return half4(fragColor);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+10" "IgnoreProjector" = "True"  }
        Cull Off
        ZTest LEqual
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Offset -5, -5
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float _Radius;
            uniform float _OutlineThickness;
            uniform float4 _Color;
            uniform float4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.uv, float2(0.5, 0.5));
                float2 ddDist = float2(ddx(dist), ddy(dist));
                float ddDistLen = rcp(length(ddDist));

                float4 innerColor = _Color;
                float4 borderColor = _OutlineColor;

                float outerRadius = _Radius;
                float innerRadius = _OutlineThickness - _Radius;

                float outerDist = dist - outerRadius;
                float outerDistOverLen = outerDist * ddDistLen;

                float innerDist = dist + innerRadius;
                float innerDistOverLen = innerDist * ddDistLen;
                
                float colorLerpParam = saturate(innerDistOverLen);
                float4 fragColor = lerp(innerColor, borderColor, colorLerpParam);
                fragColor.a *= (1.0 - saturate(outerDistOverLen));
                return fragColor;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
