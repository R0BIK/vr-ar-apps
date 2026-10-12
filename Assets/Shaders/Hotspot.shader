Shader "Unlit/Hotspot"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", COLOR) = (1,1,1,1)
        _Progress("Progress", Range(0,1)) = 0
        _Highlight("Highlight Strength", Range(0,1)) = 0
        _HighlightColor("Highlight Color", COLOR) = (1,1,1,1)
        _ShrinkLimit("Shrink Limit", float) = 0.227
        [Toggle(NORMALS_SHRINK)] _NormalsShrink("Shrink along normal", Float) = 0
        
        _OffsetFactor("Offset Factor", float) = 0
        _OffsetUnits("Offset Units", float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-10" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Offset [_OffsetFactor], [_OffsetUnits]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_local __ NORMALS_SHRINK

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
#if NORMALS_SHRINK
                float3 normalOS : NORMAL;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Progress;
                half _Highlight;
                half4 _HighlightColor;
                half _ShrinkLimit;
                float _NormalsShrink;
                float _OffsetFactor;
                float _OffsetUnits;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float prog = _Progress;
#if NORMALS_SHRINK
                float2 radius = input.normalOS.xz;
#else
                float2 radius = input.uv - float2(0.5, 0.5);
                radius = normalize(radius);
                prog = 1.0 - prog;
#endif
                half2 shrink = radius * prog * _ShrinkLimit;
                float4 pos = input.positionOS;
                pos.xz += shrink;
                float2 uv = input.uv - shrink;

                output.positionCS = TransformObjectToHClip(pos.xyz);
                output.uv = uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                col.rgb = lerp(col.rgb, _HighlightColor.rgb, _Highlight * _HighlightColor.a);
                return col;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Offset[_OffsetFactor],[_OffsetUnits]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_local __ NORMALS_SHRINK

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
#if NORMALS_SHRINK
                float3 normal : NORMAL;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            half4 _Color;
            half _Progress;
            half _Highlight;
            half4 _HighlightColor;
            half _ShrinkLimit;

            float _OffsetFactor;
            float _OffsetUnits;

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

#if NORMALS_SHRINK
                float2 radius = v.normal.xz;
#else
                float2 radius = v.uv - float2(0.5, 0.5);
                radius = normalize(radius);
                _Progress = 1 - _Progress;
#endif
                half2 shrink = radius * _Progress * _ShrinkLimit;
                v.vertex.xz += shrink;
                v.uv -= shrink;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb = lerp(col.rgb, _HighlightColor.rgb, _Highlight * _HighlightColor.a);
                return col;
            }
            ENDCG
        }
    }
}
