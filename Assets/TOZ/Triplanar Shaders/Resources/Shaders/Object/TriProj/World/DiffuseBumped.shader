Shader "TOZ/Object/TriProj/World/DiffuseBumped" {
	Properties {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Blend("Blending", Range (0.01, 0.4)) = 0.2
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 300
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		fixed4 _Color;
		sampler2D _MainTex, _BumpMap;
		float4 _MainTex_ST, _BumpMap_ST;
		fixed _Blend;

		struct Input {
			float3 weight : TEXCOORD0;
			float3 worldPos;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed3 n = max(abs(v.normal) - _Blend, 0);
			o.weight = n / (n.x + n.y + n.z).xxx;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			//Unity 5 texture interpolators already fill in limits, and no room for packing
			//So we do the uvs per pixel :(
			float2 uvx = (IN.worldPos.yz - _MainTex_ST.zw) * _MainTex_ST.xy;
			float2 uvy = (IN.worldPos.xz - _MainTex_ST.zw) * _MainTex_ST.xy;
			float2 uvz = (IN.worldPos.xy - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed4 cz = tex2D(_MainTex, uvx) * IN.weight.xxxx;
			fixed4 cy = tex2D(_MainTex, uvy) * IN.weight.yyyy;
			fixed4 cx = tex2D(_MainTex, uvz) * IN.weight.zzzz;
			fixed4 col = (cz + cy + cx) * _Color;
			o.Albedo = col.rgb;
			fixed3 bz = UnpackNormal(tex2D(_BumpMap, uvx)) * IN.weight.xxx;
			fixed3 by = UnpackNormal(tex2D(_BumpMap, uvy)) * IN.weight.yyy;
			fixed3 bx = UnpackNormal(tex2D(_BumpMap, uvz)) * IN.weight.zzz;
			o.Normal = bz + by + bx;
			o.Alpha = col.a;
		}
		ENDCG
	}

	FallBack "Legacy Shaders/Bumped Diffuse"
}