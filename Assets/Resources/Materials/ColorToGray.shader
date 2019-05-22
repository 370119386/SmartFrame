// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Test/ColorToGray" {
Properties {
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
 
SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 
 LOD 100
 Blend SrcAlpha OneMinusSrcAlpha 
 
 Pass { 
     CGPROGRAM
     #pragma vertex vert
     #pragma fragment frag
     #include "UnityCG.cginc"
 
     struct appdata_t {
         float4 vertex : POSITION;
         float2 texcoord : TEXCOORD0;
     };
 
     struct v2f {
         float4 vertex : SV_POSITION;
         half2 texcoord : TEXCOORD0;
     };
 
     sampler2D _MainTex;
     float4 _MainTex_ST;
 
     v2f vert (appdata_t v)
     {
         v2f o;
         o.vertex = UnityObjectToClipPos(v.vertex);
         o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
         return o;
     }
 
     fixed4 frag (v2f i) : SV_Target
     {
         fixed4 col = tex2D(_MainTex, i.texcoord);
         //颜色转灰度公式 = RGB 点乘 (0.3, 0.59, 0.11) = R * 0.3 + G * 0.59 + B * 0.11 = 灰度颜色标量
         fixed gray = dot(col.rgb, fixed3(0.3, 0.59, 0.11));
         col.rgb = fixed3(gray, gray, gray);
         return col;
     }
     ENDCG
 }
 }
}