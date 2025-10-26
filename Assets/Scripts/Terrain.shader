Shader "Custom/Terrain" {
	Properties {
		// Basic test texture and scale for previewing the shader
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", Float) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Use Unity’s Standard lighting model and enable shadows
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0 // Allows use of shader model 3.0 features

		const static int maxLayerCount = 8; // Max number of terrain layers supported
		const static float epsilon = 1E-4;  // Small offset to avoid floating point errors

		// Arrays used to store data for each texture layer
		int layerCount;
		float3 baseColors[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColorStrength[maxLayerCount];
		float baseTextureScales[maxLayerCount];

		float minHeight;
		float maxHeight;

		// Texture and scale for testing
		sampler2D testTexture;
		float testScale;

		// Texture array that stores all the terrain layer textures
		UNITY_DECLARE_TEX2DARRAY(baseTextures);

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		// Maps a value between a and b to a 0–1 range
		float inverseLerp(float a, float b, float value) {
			return saturate((value-a)/(b-a));
		}

		// Samples textures using triplanar projection (blends based on surface normal)
		float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
			float3 scaledWorldPos = worldPos / scale;

			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;

			return xProjection + yProjection + zProjection;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Normalised height value of the vertex
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);

			// Calculate blending weights based on surface normal direction
			float3 blendAxes = abs(IN.worldNormal);
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			// Loop through each texture layer and blend it based on height
			for (int i = 0; i < layerCount; i++) {
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseStartHeights[i]);

				float3 baseColor = baseColors[i] * baseColorStrength[i];
				float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrength[i]);

				// Blend between the layers based on draw strength
				o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
			}
		}

		ENDCG
	}

	FallBack "Diffuse"
}
