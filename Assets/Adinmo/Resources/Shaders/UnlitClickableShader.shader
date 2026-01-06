Shader "Adinmo/UnlitClickableShader"
{


    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color",Color)=(1,1,1,1)
        _EmissionMap ("Emission", 2D) = "black" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _EMISSION           

 
            #include "UnityCG.cginc"
 
            fixed4 _Color;
            fixed4 _EmissionColor;
       
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _EmissionMap;
 
            //standard 'appdata' function, but renamed to something less confusing
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 mainUv : TEXCOORD0;
            };
 
            //standard 'v2f' function, but renamed to something less confusing
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float2 mainUv : TEXCOORD0;
            };
 
       
            //take the vertex input, and
            //convert vertex data into an output that can be used by the frag function.
            //use the TRANSFORM_TEX macro to apply main texture's scale and offset values to the main UV, and decal texture's scale and offset values to the decal UV!
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.mainUv = TRANSFORM_TEX(v.mainUv, _MainTex);
                return o;
            }
 
            fixed4 frag (VertexOutput i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.mainUv);

                #if _EMISSION
                    fixed4 e = tex2D(_EmissionMap, i.mainUv);
                    c.rgb = c.rgb * _Color + e.rgb*_EmissionColor;
		#else
                    c.rgb = c.rgb * _Color;
                #endif
 
                return c;
            }
            ENDCG
        }
    }
}