// Made with Amplify Shader Editor v1.9.1.8
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "YL2/SDK/SDF/Matcap"
{
	Properties
	{
		_Matcap("Matcap", 2D) = "white" {}
		_SDF("SDF", 3D) = "white" {}
		_Dimensions("Dimensions", Vector) = (0,0,0,0)
		_BoundsMin("BoundsMin", Vector) = (0,0,0,0)
		_BoundsMax("BoundsMax", Vector) = (0,0,0,0)
		_MaxExtents("MaxExtents", Float) = 0
		[IntRange]_SDFComponentIndex("SDFComponentIndex", Float) = 0
		_Epsilon("Epsilon", Float) = -1
		[IntRange]_Steps("Steps", Range( 25 , 100)) = -1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#include "Raymarching.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Epsilon;
			uniform float _Steps;
			uniform sampler2D _Matcap;
			inline float LinearEyeDepthToOutDepth46( float z )
			{
				return LinearEyeDepthToOutDepth(-z);
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1 = v.vertex;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float localRaymarching1_g5 = ( 0.0 );
				float3 position1_g5 = i.ase_texcoord1.xyz;
				float3 worldToObjDir14 = mul( unity_WorldToObject, float4( ( WorldPosition - _WorldSpaceCameraPos ), 0 ) ).xyz;
				float3 normalizeResult13 = normalize( worldToObjDir14 );
				float3 dir1_g5 = normalizeResult13;
				float3 p1_g5 = float3( 0,0,0 );
				float3 normal1_g5 = float3( 0,0,0 );
				float use1_g5 = 0;
				float epsilon1_g5 = _Epsilon;
				int steps1_g5 = (int)_Steps;
				{
				Raymarching(position1_g5, dir1_g5, epsilon1_g5, steps1_g5, p1_g5, normal1_g5, use1_g5);
				}
				float3 objToView43 = mul( UNITY_MATRIX_MV, float4( p1_g5, 1 ) ).xyz;
				float3 viewPos44 = objToView43;
				float z46 = viewPos44.z;
				float localLinearEyeDepthToOutDepth46 = LinearEyeDepthToOutDepth46( z46 );
				float depth48 = localLinearEyeDepthToOutDepth46;
				float4 temp_cast_1 = (depth48).xxxx;
				float3 objToWorldDir23 = normalize( mul( unity_ObjectToWorld, float4( normal1_g5, 0 ) ).xyz );
				float3 worldNormal24 = objToWorldDir23;
				float3 matcapUVs26 = ( ( mul( UNITY_MATRIX_V, float4( worldNormal24 , 0.0 ) ).xyz * 0.5 ) + 0.5 );
				float4 lerpResult49 = lerp( temp_cast_1 , tex2D( _Matcap, matcapUVs26.xy ) , float4( 1,1,1,0 ));
				float use27 = use1_g5;
				clip( use27 - 0.5);
				
				
				finalColor = lerpResult49;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19108
Node;AmplifyShaderEditor.CommentaryNode;28;-2705.941,-660.2747;Inherit;False;1858.315;531;;13;10;12;13;9;11;14;23;24;27;43;44;61;63;Raymarching;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;11;-2597.277,-508.0894;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;10;-2655.941,-354.2743;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;12;-2361.942,-418.2743;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;14;-2186.864,-444.6442;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;9;-1995.712,-602.7108;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;13;-1937.937,-438.2743;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;23;-1332.299,-425.4408;Inherit;False;Object;World;True;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;21;-2661.851,471.3097;Inherit;False;1028.824;370.5427;;7;26;17;25;20;18;16;15;Matcap;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-1087.893,-415.7184;Inherit;False;worldNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewMatrixNode;20;-2531.663,521.31;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;-2600.338,625.1309;Inherit;False;24;worldNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;43;-1336.963,-592.2383;Inherit;False;Object;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;15;-2383.938,679.4707;Float;False;Constant;_Float0;Float 0;-1;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-2373.121,551.0961;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-1107.146,-592.7266;Inherit;False;viewPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;52;-2653.378,52.61382;Inherit;False;1200.604;218.0891;;4;47;46;48;51;Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-2603.378,102.6138;Inherit;False;44;viewPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-2205.871,587.5419;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;47;-2320.946,111.703;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-2025.135,636.2623;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;46;-2026.671,126.2679;Inherit;False;LinearEyeDepthToOutDepth(-z);1;Create;1;True;z;FLOAT;0;In;;Inherit;False;LinearEyeDepthToOutDepth;True;False;0;;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1840.686,653.6179;Inherit;False;matcapUVs;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;30;-1114.354,63.70874;Inherit;False;26;matcapUVs;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-1735.943,140.703;Inherit;False;depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-876.4543,48.90883;Inherit;True;Property;_Matcap;Matcap;0;0;Create;True;0;0;0;False;0;False;-1;3458853ecc7f9f542a15622189dd3e56;3458853ecc7f9f542a15622189dd3e56;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;50;-723.1179,-77.58768;Inherit;False;48;depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1335.586,-254.61;Inherit;False;use;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;49;-486.9805,43.31432;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;32;-689.6152,269.3708;Inherit;False;27;use;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;57;-498.9291,-379.5373;Inherit;False;Property;_BoundsMax;BoundsMax;4;0;Fetch;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;56;-493.2791,-524.9181;Inherit;False;Property;_BoundsMin;BoundsMin;3;0;Fetch;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;53;-733.0955,-516.7025;Inherit;True;Property;_SDF;SDF;1;0;Fetch;True;0;0;0;True;0;False;None;None;False;white;LockedToTexture3D;Texture3D;-1;0;2;SAMPLER3D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector3Node;55;-495.0287,-680.249;Inherit;False;Property;_Dimensions;Dimensions;2;0;Fetch;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClipNode;34;-255.0881,103.0144;Inherit;False;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-316.8021,-589.5803;Inherit;False;Property;_SDFComponentIndex;SDFComponentIndex;6;1;[IntRange];Fetch;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-316.3777,-679.1978;Inherit;False;Property;_MaxExtents;MaxExtents;5;0;Fetch;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;68.29919,-3.794399;Float;False;True;-1;2;ASEMaterialInspector;100;5;YL2/SDK/SDF/Matcap;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.FunctionNode;62;-1690.937,-535.2744;Inherit;False;Raymarching;-1;;5;6f14e1dc4974af3479ceaaf1934d6643;0;4;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;INT;25;False;3;FLOAT3;0;FLOAT3;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;61;-1927.481,-329.0098;Inherit;False;Property;_Epsilon;Epsilon;7;0;Create;True;0;0;0;False;0;False;-1;-1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-2050.982,-234.89;Inherit;False;Property;_Steps;Steps;8;1;[IntRange];Create;True;0;0;0;False;0;False;-1;25;25;100;0;1;FLOAT;0
WireConnection;12;0;11;0
WireConnection;12;1;10;0
WireConnection;14;0;12;0
WireConnection;13;0;14;0
WireConnection;23;0;62;2
WireConnection;24;0;23;0
WireConnection;43;0;62;0
WireConnection;18;0;20;0
WireConnection;18;1;25;0
WireConnection;44;0;43;0
WireConnection;16;0;18;0
WireConnection;16;1;15;0
WireConnection;47;0;51;0
WireConnection;17;0;16;0
WireConnection;17;1;15;0
WireConnection;46;0;47;2
WireConnection;26;0;17;0
WireConnection;48;0;46;0
WireConnection;29;1;30;0
WireConnection;27;0;62;3
WireConnection;49;0;50;0
WireConnection;49;1;29;0
WireConnection;34;0;49;0
WireConnection;34;1;32;0
WireConnection;42;0;34;0
WireConnection;62;5;9;0
WireConnection;62;6;13;0
WireConnection;62;8;61;0
WireConnection;62;9;63;0
ASEEND*/
//CHKSM=AFDACC2A10061C1BCF9F41D532741C16E2F2BFD2