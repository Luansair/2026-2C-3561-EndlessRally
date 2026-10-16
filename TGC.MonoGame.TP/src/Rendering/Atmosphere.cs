using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Tipos de superficie. El orden (0, 1, 2) tiene que coincidir con SurfaceTypeA/B del shader.
    /// </summary>
    public enum SurfaceType
    {
        Asphalt = 0,
        Dirt = 1,
        Snow = 2
    }

    /// <summary>
    ///     Mezcla de superficies de un chunk: va de From a To, y T0/T1 son el grado de mezcla
    ///     (0 = From, 1 = To) al inicio y al final del chunk. Fuera de una transicion From == To.
    /// </summary>
    public readonly struct SurfaceBlend
    {
        public readonly SurfaceType From;
        public readonly SurfaceType To;
        public readonly float T0;
        public readonly float T1;

        public SurfaceBlend(SurfaceType from, SurfaceType to, float t0, float t1)
        {
            From = from;
            To = to;
            T0 = t0;
            T1 = t1;
        }

        public float Mid => (T0 + T1) * 0.5f;
    }

    /// <summary>
    ///     Parametros de ambiente (cielo, niebla, luz, nieve, frio, agarre) para una superficie.
    /// </summary>
    public struct AtmosphereState
    {
        public Vector3 Sky;          // color de Clear() y de la niebla (0..1)
        public float FogReach;       // fraccion de la distancia de spawn donde la niebla ya es casi opaca
        public float Snow;           // 0..1  nieve sobre superficies que miran hacia arriba
        public float Cold;           // 0..1  tinte frio
        public Vector3 Ground;       // color del piso
        public Vector3 LightDiffuse; // color de la luz difusa
        public float Friction;       // agarre del vehiculo (1 = asfalto)
    }

    internal static class Atmosphere
    {
        // Que tan densa es la niebla en el borde de spawn (mas alto = mas opaca en el borde)
        private const float FogK = 1.5f;
        private static readonly Vector3 ColdTint = new Vector3(0.85f, 0.95f, 1.15f);

        public static AtmosphereState Preset(SurfaceType type)
        {
            switch (type)
            {
                case SurfaceType.Dirt:
                    return new AtmosphereState
                    {
                        Sky = new Vector3(205f, 180f, 145f) / 255f,
                        FogReach = 0.90f,
                        Snow = 0f,
                        Cold = 0f,
                        Ground = new Vector3(0.42f, 0.30f, 0.18f),
                        LightDiffuse = new Vector3(1.0f, 0.92f, 0.80f),
                        Friction = 0.75f
                    };
                case SurfaceType.Snow:
                    return new AtmosphereState
                    {
                        Sky = new Vector3(190f, 205f, 225f) / 255f,
                        FogReach = 0.70f,
                        Snow = 1f,
                        Cold = 1f,
                        Ground = new Vector3(0.85f, 0.90f, 0.95f),
                        LightDiffuse = new Vector3(0.80f, 0.90f, 1.0f),
                        Friction = 0.50f
                    };
                default: // Asphalt
                    return new AtmosphereState
                    {
                        Sky = new Vector3(110f, 160f, 230f) / 255f,
                        FogReach = 1.00f,
                        Snow = 0f,
                        Cold = 0f,
                        Ground = new Vector3(0.13f, 0.53f, 0.10f),
                        LightDiffuse = new Vector3(1.0f, 0.98f, 0.92f),
                        Friction = 1.0f
                    };
            }
        }

        public static AtmosphereState Lerp(AtmosphereState a, AtmosphereState b, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return new AtmosphereState
            {
                Sky = Vector3.Lerp(a.Sky, b.Sky, t),
                FogReach = MathHelper.Lerp(a.FogReach, b.FogReach, t),
                Snow = MathHelper.Lerp(a.Snow, b.Snow, t),
                Cold = MathHelper.Lerp(a.Cold, b.Cold, t),
                Ground = Vector3.Lerp(a.Ground, b.Ground, t),
                LightDiffuse = Vector3.Lerp(a.LightDiffuse, b.LightDiffuse, t),
                Friction = MathHelper.Lerp(a.Friction, b.Friction, t)
            };
        }

        /// <summary>Ambiente correspondiente a un punto de una mezcla de superficies.</summary>
        public static AtmosphereState Sample(SurfaceBlend blend)
        {
            return Lerp(Preset(blend.From), Preset(blend.To), blend.Mid);
        }

        /// <summary>Le pasa el ambiente al shader (una vez por frame, antes de dibujar).</summary>
        public static void Apply(Effect effect, AtmosphereState a, float spawnDistance)
        {
            float visible = Math.Max(1f, spawnDistance * a.FogReach);

            effect.Parameters["SnowCover"]?.SetValue(a.Snow);
            effect.Parameters["ColdTint"]?.SetValue(ColdTint);
            effect.Parameters["ColdAmount"]?.SetValue(a.Cold);
            effect.Parameters["FogColor"]?.SetValue(a.Sky);
            effect.Parameters["FogDensity"]?.SetValue(FogK / visible);
            effect.Parameters["lightDiffuseColor"]?.SetValue(a.LightDiffuse);
        }

        /// <summary>Apaga niebla, nieve y frio (menu, o para dibujar el auto sin nieve encima).</summary>
        public static void Clear(Effect effect)
        {
            effect.Parameters["SnowCover"]?.SetValue(0f);
            effect.Parameters["ColdAmount"]?.SetValue(0f);
            effect.Parameters["FogDensity"]?.SetValue(0f);
            effect.Parameters["UseSurfaceBlend"]?.SetValue(0f);
        }
    }
}
