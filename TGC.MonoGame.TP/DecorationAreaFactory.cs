using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    
    internal class DecorationAreaFactory
    {
        private readonly List<DecorationGroup> _natureRecipes; // Arboles y Rocas
        private readonly List<DecorationGroup> _houseRecipes;  // Casa

        public DecorationAreaFactory(List<DecorationGroup> natureRecipes, List<DecorationGroup> houseRecipes)
        {
            _natureRecipes = natureRecipes;
            _houseRecipes = houseRecipes;
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
    }
}
