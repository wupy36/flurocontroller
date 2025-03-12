// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FresnelAndOffsetNormal"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (0,0,0,1)
		_FresnelPower("FresnelPower", Range( 0 , 1)) = 0.4
		_FresnelScale("FresnelScale", Range( 0 , 3)) = 1.5
		_Bias("Bias", Float) = 0
		_PulseRate("PulseRate", Range( 0 , 10)) = 0
		_TimeOffset("TimeOffset", Range( 0 , 1)) = 0
		_NoiseScale("NoiseScale", Range( 0 , 0.03)) = 0.02
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _TimeOffset;
		uniform float _PulseRate;
		uniform float _NoiseScale;
		uniform float4 _Color;
		uniform float _Bias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float temp_output_15_0 = ( ( _Time.y + _TimeOffset ) * _PulseRate );
			float simplePerlin2D28 = snoise( ( ase_vertexNormal + (-0.2 + (temp_output_15_0 - 0.0) * (0.2 - -0.2) / (1.0 - 0.0)) ).xy );
			float3 temp_cast_1 = ((( _NoiseScale * -1.0 ) + (simplePerlin2D28 - 0.0) * (_NoiseScale - ( _NoiseScale * -1.0 )) / (1.0 - 0.0))).xxx;
			v.vertex.xyz += temp_cast_1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float temp_output_15_0 = ( ( _Time.y + _TimeOffset ) * _PulseRate );
			float fresnelNdotV1 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode1 = ( _Bias + _FresnelScale * pow( 1.0 - fresnelNdotV1, ( _FresnelPower * (0.4 + (abs( sin( temp_output_15_0 ) ) - 0.0) * (0.6 - 0.4) / (1.0 - 0.0)) ) ) );
			o.Emission = ( _Color * fresnelNode1 ).rgb;
			o.Alpha = fresnelNode1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
0;816;1199;184;2154.063;-102.2844;3.050541;True;True
Node;AmplifyShaderEditor.TimeNode;13;-1588.672,-93.11056;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-1740.618,104.6158;Float;False;Property;_TimeOffset;TimeOffset;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1580.104,243.0962;Float;False;Property;_PulseRate;PulseRate;4;0;Create;True;0;0;False;0;0;5.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-1446.251,93.82738;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1209.502,158.9963;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;16;-1064.258,131.9753;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;17;-900.9005,136.2737;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;35;-877.7361,333.2734;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;20;-716.8051,126.4475;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.4;False;4;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;40;-1055.166,611.3983;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.2;False;4;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-822.2985,-36.57368;Float;False;Property;_FresnelPower;FresnelPower;1;0;Create;True;0;0;False;0;0.4;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-743.3561,587.4247;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-836.0969,-131.054;Float;False;Property;_FresnelScale;FresnelScale;2;0;Create;True;0;0;False;0;1.5;0.39;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-535.3288,74.29333;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-743.3977,-214.128;Float;False;Property;_Bias;Bias;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-774.2104,943.3544;Float;False;Property;_NoiseScale;NoiseScale;6;0;Create;True;0;0;False;0;0.02;0.03;0;0.03;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;28;-549.116,558.4551;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;8;-495.6838,-543.0252;Float;False;Property;_Color;Color;0;1;[HDR];Create;True;0;0;False;0;0,0,0,1;11.98431,0,0.1254902,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;1;-515.1998,-288.3202;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-564.3369,829.8514;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-202.5792,-427.2099;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;38;-329.4058,561.9244;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-14,-222;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;FresnelAndOffsetNormal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;13;2
WireConnection;22;1;21;0
WireConnection;15;0;22;0
WireConnection;15;1;14;0
WireConnection;16;0;15;0
WireConnection;17;0;16;0
WireConnection;20;0;17;0
WireConnection;40;0;15;0
WireConnection;39;0;35;0
WireConnection;39;1;40;0
WireConnection;19;0;9;0
WireConnection;19;1;20;0
WireConnection;28;0;39;0
WireConnection;1;1;11;0
WireConnection;1;2;10;0
WireConnection;1;3;19;0
WireConnection;34;0;33;0
WireConnection;7;0;8;0
WireConnection;7;1;1;0
WireConnection;38;0;28;0
WireConnection;38;3;34;0
WireConnection;38;4;33;0
WireConnection;0;2;7;0
WireConnection;0;9;1;0
WireConnection;0;11;38;0
ASEEND*/
//CHKSM=8E82E7D3B1B6DF8E3AF1106D012FBD7113474877