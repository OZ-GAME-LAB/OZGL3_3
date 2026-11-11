// 이미지를 한 점에서부터 빛이 번지듯 드러내는 UI 셰이더 (UI/Default 기반)
// _Radius가 0이면 완전히 투명하고, 커질수록 중심에서부터 부드럽게 나타난다.
Shader "Elpida/UI/RadialReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _Center ("Center (0..1 in image rect)", Vector) = (0.5, 0.44, 0, 0)
        _RectInfo ("Image Rect (xMin, yMin, width, height)", Vector) = (0, 0, 1, 1)
        _Radius ("Radius", Float) = 0
        _Aspect ("Aspect (width / height)", Float) = 1.7778
        _Squash ("Squash", Float) = 0.7
        _Boost ("Boost (0..1)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float4 _Center;
            float4 _RectInfo;
            float _Radius;
            float _Aspect;
            float _Squash;
            float _Boost;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // 가로세로 비율을 맞추고, 세로로 눌러서 수면처럼 타원으로 번지게 한다.
                // 스프라이트 UV가 아니라 이미지 사각형 안의 위치(0~1)를 쓴다. 스프라이트가 아틀라스 일부여도 안전하다.
                float2 uv = (IN.worldPosition.xy - _RectInfo.xy) / max(_RectInfo.zw, float2(0.0001, 0.0001));
                float2 p = float2((uv.x - _Center.x) * _Aspect, (uv.y - _Center.y) / _Squash);
                float t = length(p) / max(_Radius, 0.0001);

                // 중심은 거의 완전히 보이고, 바깥으로 갈수록 서서히 사라진다.
                float reveal = t < 0.35 ? 1.0 - 0.08 * (t / 0.35) : 0.92 * saturate(1.0 - (t - 0.35) / 0.65);
                reveal = lerp(reveal, 1.0, _Boost);
                color.a *= reveal;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
