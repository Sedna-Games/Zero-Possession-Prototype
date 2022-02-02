Shader "TOZ/Object/TriProj/World/DiffuseParallax" {
	Properties {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Parallax("Height", Range (0.005, 0.08)) = 0.02
		_ParallaxMap("Heightmap (A)", 2D) = "black" {}
		_Blend("Blending", Range (0.01, 0.4)) = 0.2
	}

	CGINCLUDE
	fixed4 _Color;
	sampler2D _MainTex, _BumpMap, _ParallaxMap;
	float4 _MainTex_ST;
	float _Parallax;
	fixed _Blend;

	struct Input {
		float3 weight : TEXCOORD0;
		float3 worldPos;
		float3 dirView;
	};

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		fixed3 n = max(abs(v.normal) - _Blend, 0);
		o.weight = n / (n.x + n.y + n.z).xxx;
		o.dirView = WorldSpaceViewDir(v.vertex);
	}

	float2 Parallax2(half h, half height, half3 viewDir) {
		h = h * height - height/2.0;
		float3 v = normalize(viewDir);
		return h * v.xy;
	}

	void surf(Input IN, inout SurfaceOutput o) {
		//Unity 5 texture interpolators already fill in limits, and no room for packing
		//So we do the uvs per pixel :(
		float2 uvx = (IN.worldPos.yz - _MainTex_ST.zw) * _MainTex_ST.xy;
		float2 uvy = (IN.worldPos.xz - _MainTex_ST.zw) * _MainTex_ST.xy;
		float2 uvz = (IN.worldPos.xy - _MainTex_ST.zw) * _MainTex_ST.xy;

		half2 hz = Parallax2(tex2D(_ParallaxMap, uvx).w, _Parallax, IN.dirView.yzx) * IN.weight.x;
		half2 hy = Parallax2(tex2D(_ParallaxMap, uvy).w, _Parallax, IN.dirView.xzy) * IN.weight.y;
		half2 hx = Parallax2(tex2D(_ParallaxMap, uvz).w, _Parallax, IN.dirView.xyz) * IN.weight.z;
		fixed2 h = hz + hy + hx;

		fixed4 cz = tex2D(_MainTex, uvx + h) * IN.weight.xxxx;
		fixed4 cy = tex2D(_MainTex, uvy + h) * IN.weight.yyyy;
		fixed4 cx = tex2D(_MainTex, uvz + h) * IN.weight.zzzz;
		fixed4 col = (cz + cy + cx) * _Color;
		o.Albedo = col.rgb;
		fixed3 bz = UnpackNormal(tex2D(_BumpMap, uvx + h)) * IN.weight.xxx;
		fixed3 by = UnpackNormal(tex2D(_BumpMap, uvy + h)) * IN.weight.yyy;
		fixed3 bx = UnpackNormal(tex2D(_BumpMap, uvz + h)) * IN.weight.zzz;
		o.Normal = bz + by + bx;
		o.Alpha = col.a;
	}
	ENDCG

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 600
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0
		ENDCG
	}

	FallBack "Legacy Shaders/Parallax Diffuse"
}