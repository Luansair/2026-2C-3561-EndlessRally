#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 InverseTransposeWorld;

float3 DiffuseColor;

float3 lightAmbientColor; // Light's Ambient Color
float3 lightDiffuseColor; // Light's Diffuse Color
float3 lightSpecularColor; // Light's Specular Color
float KAmbient;
float KDiffuse;
float KSpecular;
float shininess;
float3 lightPosition;
float3 eyePosition; // Camera position
bool Hud = false; // true = ignora DiffuseColor , false = toma DiffuseColor

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float Time = 0;

// ---- Ambiente (niebla / nieve / frio): lo setea Atmosphere.Apply una vez por frame ----
// Con los valores por defecto (todo en 0) el resultado es identico al shader original.
float3 FogColor;
float FogDensity = 0;
float SnowCover = 0;
float3 ColdTint;
float ColdAmount = 0;

// ---- Mezcla de superficies del camino (la setea Entity.Draw para cada tramo) ----
// SurfaceTypeA / SurfaceTypeB: 0 = asfalto, 1 = tierra, 2 = nieve
float UseSurfaceBlend = 0;
float3 ChunkOrigin;
float3 ChunkForward;
float ChunkLength = 1;
float SurfaceTypeA = 0;
float SurfaceTypeB = 0;
float BlendStart = 0;
float BlendEnd = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float4 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 VertexColor : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
	output.WorldPosition = worldPosition;
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);	
	// View space to Projection space
	output.Position = mul(viewPosition, Projection);
	
	output.Normal = mul(input.Normal, InverseTransposeWorld);
	
	output.TextureCoordinates = input.TextureCoordinates;
    output.VertexColor = input.Color;
    return output;
}

// Convierte el color base del asfalto al de la superficie pedida (0 = asfalto, 1 = tierra, 2 = nieve)
float3 SurfaceAlbedo(float type, float3 baseColor)
{
    float lum = dot(baseColor, float3(0.299, 0.587, 0.114));
    float3 dirt = float3(0.50, 0.36, 0.22) * (0.55 + lum);
    float3 snow = float3(0.88, 0.92, 0.97) * (0.85 + 0.30 * lum);

    float isDirt = step(0.5, type) * (1.0 - step(1.5, type));
    float isSnow = step(1.5, type);

    return lerp(lerp(baseColor, dirt, isDirt), snow, isSnow);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal.xyz);
	
	// Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);

	// Get the texture texel
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    float3 color = Hud ? input.VertexColor.rgb : DiffuseColor;
	float3 surfaceColor = texelColor.rgb * color;

    // Mezcla de superficies a lo largo del tramo (solo la calzada: UseSurfaceBlend = 1)
    float along = saturate(dot(input.WorldPosition.xyz - ChunkOrigin, ChunkForward) / max(ChunkLength, 0.0001));
    float blend = saturate(lerp(BlendStart, BlendEnd, along));
    float3 blended = lerp(SurfaceAlbedo(SurfaceTypeA, surfaceColor), SurfaceAlbedo(SurfaceTypeB, surfaceColor), blend);
    surfaceColor = lerp(surfaceColor, blended, UseSurfaceBlend);

    // Nieve acumulada sobre lo que mira hacia arriba
    float snowAmount = SnowCover * smoothstep(0.55, 0.85, saturate(normal.y));
    surfaceColor = lerp(surfaceColor, float3(0.93, 0.96, 1.0), snowAmount);

	// Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * lightDiffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * lightSpecularColor * pow(saturate(NdotH), shininess);

    // Final calculation
    float3 ambientLight = lightAmbientColor * KAmbient;
    float3 litColor = saturate(ambientLight + diffuseLight) * surfaceColor + specularLight;

    // Tinte frio
    litColor *= lerp(float3(1.0, 1.0, 1.0), ColdTint, ColdAmount);

    // Niebla (exponencial al cuadrado, segun la distancia a la camara)
    float fogDistance = length(eyePosition - input.WorldPosition.xyz) * FogDensity;
    float fogAmount = saturate(1.0 - exp(-fogDistance * fogDistance));
    litColor = lerp(litColor, FogColor, fogAmount);

    float4 finalColor = float4(litColor, texelColor.a);
    
	return finalColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
