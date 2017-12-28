Shader "Custom/BothSides" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent"}
		//Blend SrcAlpha OneMinusSrcAlpha      
		LOD 200
        Cull Off

		CGPROGRAM
		#pragma surface surf Lambert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};


		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		//#pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            //			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color);
			o.Albedo = c;
			o.Alpha = UNITY_ACCESS_INSTANCED_PROP(_Color).a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
