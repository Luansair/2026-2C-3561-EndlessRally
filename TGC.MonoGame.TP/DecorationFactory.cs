using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    public enum DecorationType
    {
        Tree,
        Rock,
        Bush,
        Flower,
        House
    }

    internal class DecorationFactory
    {
        public static Entity Create(ModelInfo model, DecorationType type, Vector3 position, float rotation, float scale)
        {
            var entity = new Entity
            {
                Render = new RenderModel(model) { FallbackColor = GetFallbackColor(type) },
                Collider = new Collider(AABBShape.FromModel(model), isTrigger: false),
                Response = new DamageResponse(GetDamage(type)),
                IsMovable = false,
            };
            entity.Transform.Position = position;
            entity.Transform.RotationY = rotation;
            entity.Transform.Scale = scale * model.Scale;

            return entity;
        }

        public DecorationArea CreateFor(RoadPiece roadPiece, int chunkIndex)
        {
            var random = new Random(chunkIndex);

            // SI ES UNA CURVA: Colocamos una casa en la esquina externa
            if (roadPiece.rotacionY != 0)
            {
                // 50% de probabilidad de que haya casa en la curva para que no sea repetitivo
                if (random.NextDouble() > 0.5)
                {
                    return new DecorationArea(null, new List<DecorationGroup>(), random);
                }

                // Si la curva dobla a la derecha (rotacionY > 0), el exterior está a la izquierda (-X)
                // Si dobla a la izquierda (rotacionY < 0), el exterior está a la derecha (+X)
                bool doblaDerecha = roadPiece.rotacionY > 0;
                float posX = doblaDerecha ? -15f : 15f;
                float posZ = 12f; // Mitad del avance de la curva

                // Área chica (2x2) donde caerá la casa
                Shape houseShape = new RectangleShape(new Vector3(posX, 0f, posZ), 2f, 2f);

                return new DecorationArea(houseShape, _houseRecipes, random);
            }

            // 2. SI ES UNA RECTA: Bosque y rocas a los costados
            float roadHalfWidth = 5.0f;
            float decorationMargin = 6.0f;
            float outerHalfWidth = 55.0f;

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
    

        // Método auxiliar para colores de respaldo cuando el FBX no define materiales válidos
        private static Vector3 GetFallbackColor(DecorationType type) => type switch
        {
            DecorationType.Tree => new Vector3(0.20f, 0.65f, 0.20f),    // Verde hoja
            DecorationType.Rock => new Vector3(0.65f, 0.65f, 0.68f),    // Gris piedra
            DecorationType.Bush => new Vector3(0.15f, 0.55f, 0.15f),    // Verde arbusto
            DecorationType.Flower => new Vector3(0.95f, 0.30f, 0.35f),  // Tono claro flor
            DecorationType.House => new Vector3(0.85f, 0.80f, 0.75f),   // beige
            _ => Vector3.One
        };

        private static float GetDamage(DecorationType type) => type switch
        {
            DecorationType.Tree => 20f,
            DecorationType.Rock => 15f,
            DecorationType.Bush => 5f,
            DecorationType.Flower => 0f,
            DecorationType.House => 30f,
            _ => 0f
        };
    }
}
