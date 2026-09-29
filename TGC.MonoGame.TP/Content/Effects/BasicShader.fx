// =============================================================================
//  BasicShader.fx  -  TGC MonoGame TP
//
//  Reemplazo directo de Content/Effects/BasicShader.fx
//
//  Incluye:
//    1. Iluminacion Blinn-Phong (mismos parametros que ya setea TGCGame.cs)
//    2. Calzada con superficies (asfalto / tierra / nieve) y fundido entre ellas
//    3. Nieve acumulada sobre caras que miran hacia arriba (arboles, rocas, techos)
//    4. Tinte frio
//    5. Niebla exponencial cuadratica (esconde el pop-in de los chunks)
//
//  Todo lo nuevo esta APAGADO por defecto: si C# no setea un parametro nuevo,
//  vale 0 y el shader se comporta como una iluminacion comun.
// =============================================================================

#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

// -----------------------------------------------------------------------------
//  Matrices (las setea C# en cada Draw)
// -----------------------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseTransposeWorld;

// -----------------------------------------------------------------------------
//  Material
// -----------------------------------------------------------------------------
float3 DiffuseColor;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (baseTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

// -----------------------------------------------------------------------------
//  Iluminacion (se setean una vez en LoadContent, salvo lightPosition/eyePosition)
// -----------------------------------------------------------------------------
float3 lightPosition;
float3 eyePosition;

float3 lightAmbientColor;
float  KAmbient;

float3 lightDiffuseColor;
float  KDiffuse;

float3 lightSpecularColor;
float  KSpecular;
float  shininess;

// -----------------------------------------------------------------------------
//  Superficie de la calzada (NUEVO)
//  Se setean por chunk, SOLO mientras se dibuja la parte de asfalto.
//  Para el resto de las partes / objetos: UseSurfaceBlend = 0.
// -----------------------------------------------------------------------------
float  UseSurfaceBlend;   // 1 = esta parte es la calzada, 0 = material normal
float  SurfaceTypeA;      // superficie de origen : 0 asfalto, 1 tierra, 2 nieve
float  SurfaceTypeB;      // superficie de destino: 0 asfalto, 1 tierra, 2 nieve
float  BlendStart;        // mezcla (0..1) al inicio del chunk
float  BlendEnd;          // mezcla (0..1) al final del chunk
float3 ChunkOrigin;       // posicion en mundo del inicio del chunk
float3 ChunkForward;      // direccion en mundo del eje local +Z del chunk (normalizada)
float  ChunkLength;       // largo del chunk (offsetLocal.Z de la pieza)

// -----------------------------------------------------------------------------
//  Ambiente (NUEVO)
// -----------------------------------------------------------------------------
float  SnowCover;         // 0..1  nieve acumulada sobre superficies que miran hacia arriba
float3 ColdTint;          // multiplicador de color frio, ej. (0.85, 0.95, 1.15)
float  ColdAmount;        // 0..1  cuanto se aplica el tinte frio
float3 FogColor;          // conviene igualarlo al color de Clear()
float  FogDensity;        // 0 = sin niebla. Regla: ~ 2.5 / distanciaVisible

// -----------------------------------------------------------------------------
//  Paleta de superficies procedurales
// -----------------------------------------------------------------------------
#define DIRT_DARK   float3(0.30, 0.20, 0.11)
#define DIRT_LIGHT  float3(0.52, 0.38, 0.24)
#define SNOW_COLOR  float3(0.93, 0.96, 1.00)

// -----------------------------------------------------------------------------
//  Ruido procedural (no necesita texturas)
// -----------------------------------------------------------------------------
float Hash21(float2 p)
{
	float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return frac((p3.x + p3.y) * p3.z);
}

float ValueNoise(float2 p)
{
	float2 i = floor(p);
	float2 f = frac(p);
	f = f * f * (3.0 - 2.0 * f);

	float a = Hash21(i);
	float b = Hash21(i + float2(1.0, 0.0));
	float c = Hash21(i + float2(0.0, 1.0));
	float d = Hash21(i + float2(1.0, 1.0));

	return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// Dos octavas, resultado en 0..1
float Fbm(float2 p)
{
	return ValueNoise(p) * 0.65 + ValueNoise(p * 2.13) * 0.35;
}

// -----------------------------------------------------------------------------
//  Materiales
// -----------------------------------------------------------------------------
float3 DirtAlbedo(float3 wp)
{
	float3 c = lerp(DIRT_DARK, DIRT_LIGHT, Fbm(wp.xz * 0.7));
	c *= 0.88 + 0.24 * Hash21(floor(wp.xz * 9.0)); // piedritas
	return c;
}

float3 SnowAlbedo(float3 wp)
{
	float3 c = SNOW_COLOR * (0.93 + 0.07 * Fbm(wp.xz * 0.4));
	float glint = smoothstep(0.985, 1.0, Hash21(floor(wp.xz * 22.0)));
	return c + glint * 0.35; // destellos
}

// Pesos (asfalto, tierra, nieve) para un tipo 0/1/2 sin usar ifs
float3 TypeWeights(float type)
{
	return saturate(1.0 - abs(type - float3(0.0, 1.0, 2.0)));
}

// -----------------------------------------------------------------------------
//  Vertex shader
// -----------------------------------------------------------------------------
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position      : SV_POSITION;
	float3 WorldPosition : TEXCOORD0;
	float3 Normal        : TEXCOORD1;
	float2 TexCoord      : TEXCOORD2;
	float  SurfaceT      : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition  = mul(worldPosition, View);

	output.Position      = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition.xyz;
	output.Normal        = mul(float4(input.Normal, 0.0), InverseTransposeWorld).xyz;
	output.TexCoord      = input.TexCoord;

	// Avance (0..1) a lo largo del chunk, medido en unidades de mundo:
	// no depende de la escala ni del origen del modelo.
	float s = dot(worldPosition.xyz - ChunkOrigin, ChunkForward) / max(ChunkLength, 0.001);
	output.SurfaceT = lerp(BlendStart, BlendEnd, saturate(s));

	return output;
}

// -----------------------------------------------------------------------------
//  Pixel shader
// -----------------------------------------------------------------------------
float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 wp = input.WorldPosition;

	// Se muestrea siempre (fuera de cualquier branch)
	float3 modelColor = tex2D(textureSampler, input.TexCoord).rgb * DiffuseColor;

	// Geometria sin normales (los gizmos de hitbox): color plano, sin luz ni niebla
	if (dot(input.Normal, input.Normal) < 0.0001)
		return float4(DiffuseColor, 1.0);

	float3 N = normalize(input.Normal);
	float3 albedo = modelColor;
	float  specMul = 1.0;

	// ---- Calzada: mezcla entre superficies ---------------------------------
	if (UseSurfaceBlend > 0.5)
	{
		float3 wA = TypeWeights(SurfaceTypeA);
		float3 wB = TypeWeights(SurfaceTypeB);

		// El asfalto conserva el material original del modelo, con un poco de grano
		float3 matAsphalt = modelColor * (0.93 + 0.14 * ValueNoise(wp.xz * 6.0));
		float3 matDirt    = DirtAlbedo(wp);
		float3 matSnow    = SnowAlbedo(wp);

		float3 colA = matAsphalt * wA.x + matDirt * wA.y + matSnow * wA.z;
		float3 colB = matAsphalt * wB.x + matDirt * wB.y + matSnow * wB.z;

		// El ruido rompe el fundido en manchones en vez de un crossfade parejo.
		// Con amplitud +-0.3 los extremos quedan exactos (t=0 -> A, t=1 -> B).
		float t  = input.SurfaceT;
		float nb = Fbm(wp.xz * 0.35);
		float tn = smoothstep(0.35, 0.65, t + (nb - 0.5) * 0.6);

		albedo = lerp(colA, colB, tn);

		// Brillo especular por superficie (asfalto, tierra, nieve)
		float3 specs = float3(0.6, 0.0, 0.5);
		specMul = lerp(dot(wA, specs), dot(wB, specs), tn);
	}
	else
	{
		// ---- Nieve acumulada sobre caras que miran hacia arriba ------------
		float upFacing = saturate(N.y * 1.6 - 0.5);
		float n = Fbm(wp.xz * 1.3);
		float snowMask = saturate((SnowCover * (upFacing + (n - 0.5) * 0.4) - 0.25) * 4.0);
		albedo = lerp(albedo, SNOW_COLOR, snowMask);
		specMul = lerp(specMul, 0.5, snowMask);
	}

	// ---- Iluminacion Blinn-Phong -------------------------------------------
	float3 L = normalize(lightPosition - wp);
	float3 V = normalize(eyePosition - wp);
	float3 H = normalize(L + V);

	float NdotL = saturate(dot(N, L));
	float spec  = pow(saturate(dot(N, H)), shininess) * (NdotL > 0.0 ? 1.0 : 0.0);

	float3 ambient  = KAmbient * lightAmbientColor;
	float3 diffuse  = KDiffuse * lightDiffuseColor * NdotL;
	float3 specular = KSpecular * lightSpecularColor * spec * specMul;

	float3 color = albedo * (ambient + diffuse) + specular;

	// ---- Tinte frio ---------------------------------------------------------
	color = lerp(color, color * ColdTint, ColdAmount);

	// ---- Niebla exponencial cuadratica -------------------------------------
	float viewDist = length(eyePosition - wp);
	float x = viewDist * FogDensity;
	float fog = saturate(1.0 - exp(-x * x));
	color = lerp(color, FogColor, fog);

	return float4(saturate(color), 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader  = compile PS_SHADERMODEL MainPS();
	}
};
