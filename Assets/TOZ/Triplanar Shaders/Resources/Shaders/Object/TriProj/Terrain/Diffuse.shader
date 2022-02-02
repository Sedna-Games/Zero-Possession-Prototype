// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TOZ/Object/TriProj/Terrain/Diffuse" {
	Properties {
		_Blend("Blending", Range (0.01, 0.4)) = 0.2
		_TextureScale("Texture Scale", Float) = 500.0

		// set by terrain engine
		[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
		//used in fallback on old cards & base map
		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color("Main Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags { "SplatCount" = "4" "Queue" = "Geometry-99" "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert finalcolor:myfinal exclude_path:prepass exclude_path:deferred
		#pragma multi_compile_fog
		#pragma multi_compile __ _TERRAIN_NORMAL_MAP

		sampler2D _Control;
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		#ifdef _TERRAIN_NORMAL_MAP
			sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
		#endif
		float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
		fixed _Blend;
		float _TextureScale;

		struct Input {
			float2 uv_Control : TEXCOORD0;
			float3 norm : TEXCOORD1;
			float3 worldPos;
			UNITY_FOG_COORDS(2)
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed3 n = max(abs(v.normal) - _Blend, 0);
			o.norm = n / (n.x + n.y + n.z).xxx;
			float4 pos = UnityObjectToClipPos (v.vertex);
			UNITY_TRANSFER_FOG(o, pos);
			#ifdef _TERRAIN_NORMAL_MAP
				v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
				v.tangent.w = -1;
			#endif
		}

		void surf (Input IN, inout SurfaceOutput o) {
			//Unity 5 texture interpolators already fill in limits, and no room for packing
			//So we do the uvs per pixel :(
			float3 oPos = mul(unity_WorldToObject, fixed4(IN.worldPos, 1.0)).xyz * (1.0 / _TextureScale);

			fixed4 splat_control = tex2D(_Control, IN.uv_Control);
			half weight = dot(splat_control, half4(1, 1, 1, 1));
			#ifndef UNITY_PASS_DEFERRED
				splat_control /= (weight + 1e-3f); // avoid NaNs in splat_control
			#endif
			#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
				clip(weight - 0.0039 /*1/255*/);
			#endif

			float2 uvx, uvy, uvz;
			fixed4 cx, cy, cz, col;
			fixed4 nx, ny, nz, nrm;

			//Splat0
			uvx = (oPos.yz - _Splat0_ST.zw) * _Splat0_ST.xy;
			uvy = (oPos.xz - _Splat0_ST.zw) * _Splat0_ST.xy;
			uvz = (oPos.xy - _Splat0_ST.zw) * _Splat0_ST.xy;
			cx = (splat_control.r * tex2D(_Splat0, uvx)) * IN.norm.xxxx;
			cy = (splat_control.r * tex2D(_Splat0, uvy)) * IN.norm.yyyy;
			cz = (splat_control.r * tex2D(_Splat0, uvz)) * IN.norm.zzzz;
			col = (cx + cy + cz);
			#ifdef _TERRAIN_NORMAL_MAP
				nx = (splat_control.r * tex2D(_Normal0, uvx)) * IN.norm.xxxx;
				ny = (splat_control.r * tex2D(_Normal0, uvy)) * IN.norm.yyyy;
				nz = (splat_control.r * tex2D(_Normal0, uvz)) * IN.norm.zzzz;
				nrm = (nx + ny + nz);
			#endif

			//Splat1
			uvx = (oPos.yz - _Splat1_ST.zw) * _Splat1_ST.xy;
			uvy = (oPos.xz - _Splat1_ST.zw) * _Splat1_ST.xy;
			uvz = (oPos.xy - _Splat1_ST.zw) * _Splat1_ST.xy;
			cx = (splat_control.g * tex2D(_Splat1, uvx)) * IN.norm.xxxx;
			cy = (splat_control.g * tex2D(_Splat1, uvy)) * IN.norm.yyyy;
			cz = (splat_control.g * tex2D(_Splat1, uvz)) * IN.norm.zzzz;
			col += (cx + cy + cz);
			#ifdef _TERRAIN_NORMAL_MAP
				nx = (splat_control.g * tex2D(_Normal1, uvx)) * IN.norm.xxxx;
				ny = (splat_control.g * tex2D(_Normal1, uvy)) * IN.norm.yyyy;
				nz = (splat_control.g * tex2D(_Normal1, uvz)) * IN.norm.zzzz;
				nrm += (nx + ny + nz);
			#endif

			//Splat2
			uvx = (oPos.yz - _Splat2_ST.zw) * _Splat2_ST.xy;
			uvy = (oPos.xz - _Splat2_ST.zw) * _Splat2_ST.xy;
			uvz = (oPos.xy - _Splat2_ST.zw) * _Splat2_ST.xy;
			cx = (splat_control.b * tex2D(_Splat2, uvx)) * IN.norm.xxxx;
			cy = (splat_control.b * tex2D(_Splat2, uvy)) * IN.norm.yyyy;
			cz = (splat_control.b * tex2D(_Splat2, uvz)) * IN.norm.zzzz;
			col += (cx + cy + cz);
			#ifdef _TERRAIN_NORMAL_MAP
				nx = (splat_control.b * tex2D(_Normal2, uvx)) * IN.norm.xxxx;
				ny = (splat_control.b * tex2D(_Normal2, uvy)) * IN.norm.yyyy;
				nz = (splat_control.b * tex2D(_Normal2, uvz)) * IN.norm.zzzz;
				nrm += (nx + ny + nz);
			#endif

			//Splat3
			uvx = (oPos.yz - _Splat3_ST.zw) * _Splat3_ST.xy;
			uvy = (oPos.xz - _Splat3_ST.zw) * _Splat3_ST.xy;
			uvz = (oPos.xy - _Splat3_ST.zw) * _Splat3_ST.xy;
			cx = (splat_control.a * tex2D(_Splat3, uvx)) * IN.norm.xxxx;
			cy = (splat_control.a * tex2D(_Splat3, uvy)) * IN.norm.yyyy;
			cz = (splat_control.a * tex2D(_Splat3, uvz)) * IN.norm.zzzz;
			col += (cx + cy + cz);
			#ifdef _TERRAIN_NORMAL_MAP
				nx = (splat_control.a * tex2D(_Normal3, uvx)) * IN.norm.xxxx;
				ny = (splat_control.a * tex2D(_Normal3, uvy)) * IN.norm.yyyy;
				nz = (splat_control.a * tex2D(_Normal3, uvz)) * IN.norm.zzzz;
				nrm += (nx + ny + nz);
			#endif

			//Sum
			col.rgb *= weight;
			o.Albedo = col.rgb;
			#ifdef _TERRAIN_NORMAL_MAP
			o.Normal = UnpackNormal(nrm);
			#endif
			o.Alpha = 1.0;
		}

		void myfinal(Input IN, SurfaceOutput o, inout fixed4 color) {
			//Add fog last
			#ifdef TERRAIN_SPLAT_ADDPASS
				UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0, 0, 0, 0));
			#else
				UNITY_APPLY_FOG(IN.fogCoord, color);
			#endif
		}
		ENDCG
	}

	Dependency "AddPassShader" = "TOZ/Object/TriProj/Terrain/Diffuse-AddPass"
	Dependency "BaseMapShader" = "Diffuse"
	Dependency "Details0" = "Hidden/TerrainEngine/Details/Vertexlit"
	Dependency "Details1" = "Hidden/TerrainEngine/Details/WavingDoublePass"
	Dependency "Details2" = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
	Dependency "Tree0" = "Hidden/TerrainEngine/BillboardTree"

	Fallback "Nature/Terrain/Diffuse"
}