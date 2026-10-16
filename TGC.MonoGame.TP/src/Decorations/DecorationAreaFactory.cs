using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Road;

namespace TGC.MonoGame.TP.src.Decorations
{

    internal class DecorationAreaFactory
    {
        private readonly List<DecorationGroup> _natureRecipes; // Arboles y Rocas
        private readonly List<DecorationGroup> _houseRecipes;  // Casa

        // Distancia lateral de la casa respecto al eje de la curva
        // Donde empieza el area de casas, medido desde el eje del camino hacia afuera de la curva
        private const float CurveHouseInnerDistance = 25f;
        // Ancho del area hacia afuera (en X). El largo (Z) es el de la curva.
        private const float CurveHouseAreaWidth = 70f;
        // Distancia minima entre casas: subila si se tocan, bajala si querés mas casas
        private const float CurveHouseMinSpacing = 12f;

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
                // La curva mide ~37.5 de radio (15 * escala 2.5), asi que la casa va bien afuera del asfalto
                float centerX = CurveHouseInnerDistance + CurveHouseAreaWidth / 2f;
                float posX = doblaDerecha ? -centerX : centerX;
                float posZ = roadPiece.offsetLocal.Z / 2f; // Mitad del avance de la curva

                // Area donde caeran las casas (mas grande que antes para que no se apilen)
                Shape houseShape = new RectangleShape(new Vector3(posX, 0f, posZ), CurveHouseAreaWidth, roadPiece.offsetLocal.Z);

                return new DecorationArea(houseShape, _houseRecipes, random, CurveHouseMinSpacing);
            }

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
    }
}