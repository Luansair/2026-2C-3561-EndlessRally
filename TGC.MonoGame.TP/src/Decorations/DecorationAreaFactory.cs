using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Road;

namespace TGC.MonoGame.TP.src.Decorations
{

    internal class DecorationAreaFactory
    {
        private readonly List<DecorationGroup> _natureRecipes; // Arboles y Rocas
        private readonly List<DecorationGroup> _houseRecipes;  // Casa

        // ---- Relleno de curvas (todo en coordenadas locales de la pieza, mismas que CornerPath) ----
        // Distancia del eje del camino al centro de la hilera de barriers
        private const float CurveBarrierOffset = 12f;
        // Hasta donde se extiende el relleno (medido desde el eje del camino)
        private const float CurveFillDepth = 75f;
        // Donde empiezan las casas (mas lejos que los arboles)
        private const float CurveHouseStart = 30f;
        private const float CurveTreeMinSpacing = 7f;
        private const float CurveHouseMinSpacing = 16f;
        private const int CurveTreeAttempts = 40;
        private const int CurveHouseAttempts = 8;
        // true = tambien tapa el lado de adentro de la curva (barriers + arboles)
        private const bool FillInsideOfCurve = true;

        private readonly DecorationGroup _barrierRecipe;

        public DecorationAreaFactory(List<DecorationGroup> natureRecipes, List<DecorationGroup> houseRecipes, DecorationGroup barrierRecipe)
        {
            _natureRecipes = natureRecipes;
            _houseRecipes = houseRecipes;
            _barrierRecipe = barrierRecipe;
        }

        public DecorationArea CreateFor(RoadPiece roadPiece, int chunkIndex)
        {
            var random = new Random(chunkIndex);

            // SI ES UNA CURVA: el relleno se genera aparte (CreateCurveFill), porque sigue el arco
            if (roadPiece.rotacionY != 0)
                return new DecorationArea(null, new List<DecorationGroup>(), random);

            // 2. SI ES UNA RECTA: Bosque y rocas a los costados
            float roadHalfWidth = 5.0f;
            float decorationMargin = 14.0f;   // antes 6: separacion minima entre el borde del camino y la primera decoracion
            float outerHalfWidth = 90.0f;     // antes 55: hasta donde llegan las decoraciones

            float safeStart = roadHalfWidth + decorationMargin;
            float sideWidth = outerHalfWidth - safeStart;

            float rectangleMiddlePosX = safeStart + (sideWidth / 2f);
            float centerZ = roadPiece.offsetLocal.Z / 2f;

            var leftPos = new Vector3(-rectangleMiddlePosX, 0, centerZ);
            var rightPos = new Vector3(rectangleMiddlePosX, 0, centerZ);

            Shape left = new RectangleShape(leftPos, sideWidth, roadPiece.offsetLocal.Z);
            Shape right = new RectangleShape(rightPos, sideWidth, roadPiece.offsetLocal.Z);

            Shape natureShape = new CompositeShape(left, right);

            return new DecorationArea(natureShape, _natureRecipes, random);
        }

        /// <summary>
        ///     Rellena una curva: hilera de barriers siguiendo el arco + arboles y casas detras.
        ///     Devuelve entidades en coordenadas LOCALES de la pieza (RoadSpawner las pasa a mundo).
        /// </summary>
        public List<Entity> CreateCurveFill(RoadPiece piece, int chunkIndex)
        {
            var result = new List<Entity>();
            var random = new Random(chunkIndex * 7919 + 13);

            // Misma geometria que RoadSpawner.CornerPath
            float dir = MathF.Sign(piece.offsetLocal.X);
            if (dir == 0f) dir = 1f;
            float radius = (MathF.Abs(piece.offsetLocal.X) + MathF.Abs(piece.offsetLocal.Z)) / 2f;
            var center = new Vector3(dir * MathF.Abs(piece.offsetLocal.X), 0f, 0f);

            // side = +1 -> exterior de la curva, side = -1 -> interior
            for (int side = 1; side >= (FillInsideOfCurve ? -1 : 1); side -= 2)
            {
                float barrierRadius = radius + side * CurveBarrierOffset;
                if (barrierRadius <= 0f) continue;

                AddBarrierRow(result, center, dir, barrierRadius);

                // Zona detras de los barriers
                float treeInner = barrierRadius + side * 4f;
                float treeOuter = side > 0 ? radius + CurveFillDepth : MathF.Max(0f, radius - CurveFillDepth);
                AddScattered(result, random, center, dir, treeInner, treeOuter, _natureRecipes,
                             CurveTreeAttempts, CurveTreeMinSpacing);

                if (side > 0) // casas solo afuera, mas lejos que los arboles
                    AddScattered(result, random, center, dir, radius + CurveHouseStart, radius + CurveFillDepth,
                                 _houseRecipes, CurveHouseAttempts, CurveHouseMinSpacing);
            }
            return result;
        }

        // Punto del arco a "angle" y distancia "r" del centro de giro
        private static Vector3 PointOnArc(Vector3 center, float dir, float angle, float r)
        {
            // n = direccion centro -> punto sobre la curva
            var n = new Vector3(-dir * MathF.Cos(angle), 0f, MathF.Sin(angle));
            return center + n * r;
        }

        private void AddBarrierRow(List<Entity> result, Vector3 center, float dir, float r)
        {
            if (_barrierRecipe == null) return;

            ModelInfo model = _barrierRecipe.GetRandomModel(new Random(0));
            // largo real de un barrier = ancho del bounding box en X * escala
            float length = (model.LocalBounds.Max.X - model.LocalBounds.Min.X) * model.Scale;
            float arcLength = r * MathHelper.PiOver2;
            int count = Math.Max(1, (int)MathF.Ceiling(arcLength / length));
            float step = MathHelper.PiOver2 / count;

            for (int i = 0; i < count; i++)
            {
                float angle = (i + 0.5f) * step;
                Vector3 pos = PointOnArc(center, dir, angle, r);

                // Tangente del arco: d/da (dir*(1-cos a), sin a) = (dir*sin a, cos a)
                var tangent = new Vector3(dir * MathF.Sin(angle), 0f, MathF.Cos(angle));
                // CreateRotationY(t) manda +X a (cos t, 0, -sin t) -> alineamos el largo del barrier con la tangente
                float yaw = MathF.Atan2(-tangent.Z, tangent.X);

                result.Add(DecorationFactory.Create(model, DecorationType.Barrier, pos, yaw, 1f));
            }
        }

        // Reparte decoraciones al azar en el sector anular [rMin, rMax] del arco
        private static void AddScattered(List<Entity> result, Random random, Vector3 center, float dir,
                                         float rA, float rB, List<DecorationGroup> recipes, int attempts, float minSpacing)
        {
            float rMin = MathF.Min(rA, rB), rMax = MathF.Max(rA, rB);
            if (rMax - rMin < 1f) return;
            var placedHere = new List<Vector3>();

            foreach (var recipe in recipes)
            {
                // "Amount" de la receta se usa como tope, pero limitado por "attempts"
                int tries = Math.Min(recipe.Amount, attempts);
                for (int i = 0; i < tries; i++)
                {
                    float angle = random.NextSingle() * MathHelper.PiOver2;
                    float r = rMin + random.NextSingle() * (rMax - rMin);
                    Vector3 pos = PointOnArc(center, dir, angle, r);

                    bool tooClose = false;
                    foreach (var q in placedHere)
                    {
                        float dx = q.X - pos.X, dz = q.Z - pos.Z;
                        if (dx * dx + dz * dz < minSpacing * minSpacing) { tooClose = true; break; }
                    }
                    if (tooClose) continue;
                    placedHere.Add(pos);

                    float rot = random.NextSingle() * MathHelper.TwoPi;
                    float scale = 0.75f + random.NextSingle() * 0.5f;
                    result.Add(DecorationFactory.Create(recipe.GetRandomModel(random), recipe.Type, pos, rot, scale));
                }
            }
        }
    }
}