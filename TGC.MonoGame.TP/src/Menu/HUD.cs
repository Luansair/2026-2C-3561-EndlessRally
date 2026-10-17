using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.src.Core;
internal class HUD
{
    static readonly Color Frame  = new Color(160, 160, 160); // gris cordón
    static readonly Color Panel  = new Color(45, 45, 45);    // gris asfalto
    static readonly Color Off    = new Color(70, 70, 70);
    static readonly Color Green  = new Color(46, 160, 46);
    static readonly Color Yellow = new Color(240, 190, 40);
    static readonly Color Red    = new Color(210, 50, 40);
    static readonly Color Amber  = new Color(255, 160, 30);
    static readonly Color White  = new Color(240, 240, 240);
    // ---------- 1) VIDA ----------
    public static VertexPositionColor[] CreateHealthBar(Vector2 pos, Vector2 size, float health01)
    {
        var v = new List<VertexPositionColor>();
        health01 = MathHelper.Clamp(health01, 0f, 1f);
        const int cells = 10;
        float b = size.Y * 0.15f;

        Quad(v, pos.X, pos.Y, size.X, size.Y, Frame);
        Quad(v, pos.X + b, pos.Y + b, size.X - 2 * b, size.Y - 2 * b, Panel);

        float x0 = pos.X + 2 * b, y0 = pos.Y + 2 * b;
        float iw = size.X - 4 * b, ih = size.Y - 4 * b;
        float gap = b * 0.5f;
        float cw = (iw - gap * (cells - 1)) / cells;
        int filled = (int)Math.Ceiling(health01 * cells);
        Color on = health01 > 0.5f ? Green : health01 > 0.25f ? Yellow : Red;

        for (int i = 0; i < cells; i++)
            Quad(v, x0 + i * (cw + gap), y0, cw, ih, i < filled ? on : Off);

        return v.ToArray();
    }
    public static void DrawHealth(Effect _effect, GraphicsDevice GraphicsDevice, int health)
    {
        var healthVertices = CreateHealthBar(
            new Vector2(-0.98f, 0.93f),
            new Vector2(0.25f, 0.05f),
            health01: health / 100f
        );

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                healthVertices,
                0,
                healthVertices.Length / 3
            );
        }
    }
    // ---------- 2) PUNTOS ----------
    public static VertexPositionColor[] CreateScore(Vector2 pos, float digitHeight, int score, int digits = 6)
    {
        var v = new List<VertexPositionColor>();
        int max = (int)Math.Pow(10, digits) - 1;
        score = Math.Max(0, Math.Min(score, max));

        string[] masks = { "abcdef", "bc", "abdeg", "abcdg", "bcfg",
                           "acdfg", "acdefg", "abc", "abcdefg", "abcdfg" };
        float h = digitHeight, w = h * 0.55f, t = h * 0.14f;
        float sp = h * 0.2f, pad = h * 0.3f, b = h * 0.1f;
        float totalW = digits * w + (digits - 1) * sp + 2 * pad;
        float totalH = h + 2 * pad;

        Quad(v, pos.X, pos.Y, totalW, totalH, Frame);
        Quad(v, pos.X + b, pos.Y + b, totalW - 2 * b, totalH - 2 * b, Panel);

        string s = score.ToString().PadLeft(digits, '0');
        float sh = h / 2f - 1.5f * t;
        for (int i = 0; i < digits; i++)
        {
            float x = pos.X + pad + i * (w + sp), y = pos.Y + pad;
            string m = masks[s[i] - '0'];
            foreach (char c in "abcdefg")
            {
                Color col = m.IndexOf(c) >= 0 ? White : Off;
                switch (c)
                {
                    case 'a': Quad(v, x + t, y, w - 2 * t, t, col); break;
                    case 'g': Quad(v, x + t, y + h / 2 - t / 2, w - 2 * t, t, col); break;
                    case 'd': Quad(v, x + t, y + h - t, w - 2 * t, t, col); break;
                    case 'f': Quad(v, x, y + t, t, sh, col); break;
                    case 'b': Quad(v, x + w - t, y + t, t, sh, col); break;
                    case 'e': Quad(v, x, y + h / 2 + t / 2, t, sh, col); break;
                    case 'c': Quad(v, x + w - t, y + h / 2 + t / 2, t, sh, col); break;
                }
            }
        }
        return v.ToArray();
    }

    public static void DrawScore(Effect _effect, GraphicsDevice GraphicsDevice, int score)
    {
        var scoreVertices = CreateScore(
        new Vector2(0.75f, 0.90f),
        digitHeight: 0.05f,
        score: score
    );

    foreach (var pass in _effect.CurrentTechnique.Passes)
    {
        pass.Apply();
        GraphicsDevice.DrawUserPrimitives(
            PrimitiveType.TriangleList,
            scoreVertices,
            0,
            scoreVertices.Length / 3
        );
    }
    }
    // ---------- 3) COMBUSTIBLE ----------
    // refactorizar geometria
    public static VertexPositionColor[] CreateFuelGauge(Vector2 center, float radius, float fuel01, float maxFuel)
    {
        var v = new List<VertexPositionColor>();
        fuel01 = fuel01 / maxFuel;

        const int fan = 120;   // triángulos del semicírculo
        const int segs = 8;    // segmentos del medidor
        const int sub = 8;     // subdivisiones de curvatura por segmento
        const float ys = -1f;  

        // Punto sobre el arco: ángulo de PI (izquierda) a 2PI (derecha), pasando por ARRIBA
        Vector2 P(float r, float a) =>
            new Vector2(center.X + (float)Math.Cos(a) * r, center.Y + (float)Math.Sin(a) * r * ys);

        void Fan(float r, Color col)
        {
            for (int i = 0; i < fan; i++)
                Tri(v, center,
                    P(r, MathHelper.Pi + i * MathHelper.Pi / fan),
                    P(r, MathHelper.Pi + (i + 1) * MathHelper.Pi / fan), col);
        }

        Fan(radius, Frame);
        Fan(radius * 0.88f, Panel);

        float bh = radius * 0.12f;                           // base plana (siempre debajo del centro)
        Quad(v, center.X - radius, ys > 0 ? center.Y : center.Y - bh, radius * 2, bh, Frame);

        int filled = (int)Math.Ceiling(fuel01 * segs);
        Color on = fuel01 > 0.25f ? Amber : Red;
        float rIn = radius * 0.55f, rOut = radius * 0.80f;
        float da = MathHelper.Pi / segs, g = da * 0.12f;

        for (int i = 0; i < segs; i++)
        {
            Color c = i < filled ? on : Off;
            float s0 = MathHelper.Pi + i * da + g;
            float span = da - 2 * g;
            for (int j = 0; j < sub; j++)
            {
                float a0 = s0 + span * j / sub, a1 = s0 + span * (j + 1) / sub;
                Vector2 i0 = P(rIn, a0), o0 = P(rOut, a0), i1 = P(rIn, a1), o1 = P(rOut, a1);
                Tri(v, i0, o0, o1, c);
                Tri(v, i0, o1, i1, c);
            }
        }

        // Aguja: 0 = izquierda (E), 1 = derecha (F)
        float an = MathHelper.Pi + fuel01 * MathHelper.Pi;
        Vector2 dir = new Vector2((float)Math.Cos(an), (float)Math.Sin(an) * ys);
        Vector2 perp = new Vector2(-dir.Y, dir.X) * radius * 0.04f;
        Tri(v, center + perp, center + dir * radius * 0.85f, center - perp, White);

        float hub = radius * 0.07f;
        Quad(v, center.X - hub, center.Y - hub, hub * 2, hub * 2, Frame);

        return v.ToArray();
    }
    public static void DrawFuel(Effect _effect, GraphicsDevice GraphicsDevice, float fuel,float maxFuel)
    {
        var fuelVertices = CreateFuelGauge(
            new Vector2(0.75f, -0.90f),
            radius: 0.2f,
            fuel01: fuel,
            maxFuel: maxFuel
        );

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                fuelVertices,
                0,
                fuelVertices.Length / 3
            );
        }
    }
    // ---------- helpers ----------
    static Vector2 Polar(Vector2 c, float r, float a) =>
        c + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r;

    static void Tri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c, Color col)
    {
        v.Add(new VertexPositionColor(new Vector3(a, 0), col));
        v.Add(new VertexPositionColor(new Vector3(b, 0), col));
        v.Add(new VertexPositionColor(new Vector3(c, 0), col));
    }

    static void Quad(List<VertexPositionColor> v, float x, float y, float w, float h, Color col)
    {
        Tri(v, new Vector2(x, y), new Vector2(x + w, y), new Vector2(x, y + h), col);
        Tri(v, new Vector2(x + w, y), new Vector2(x + w, y + h), new Vector2(x, y + h), col);
    }

    static void HalfDisc(List<VertexPositionColor> v, Vector2 c, float r, int n, Color col)
    {
        for (int i = 0; i < n; i++)
        {
            float a0 = MathHelper.Pi + i * MathHelper.Pi / n;
            float a1 = MathHelper.Pi + (i + 1) * MathHelper.Pi / n;
            Tri(v, c, Polar(c, r, a0), Polar(c, r, a1), col);
        }
    }
}