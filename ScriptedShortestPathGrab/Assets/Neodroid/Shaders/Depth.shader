Shader "Neodroid/DepthCameraShader" {
  Properties{
  }
  SubShader{
    Tags{ "RenderType" = "Opaque" }

    Pass{
      Fog{ Mode Off }
      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _CameraDepthTexture;

        struct v2f {
          float4 pos : SV_POSITION;
          float4 uv: TEXCOORD1;
        };

        v2f vert(appdata_base v) {
          v2f o;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.uv = ComputeScreenPos(o.pos);
          return o;
        }

        half4 frag(v2f i) : COLOR {
          float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
          half4 color = half4(depth, depth, depth, 1);
          return color;
        }
      ENDCG
    }
  }
    FallBack "Diffuse"
}