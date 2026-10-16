using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;


namespace TGC.MonoGame.TP.src.Decorations
{
    internal class DecorationArea
    {
        private readonly Shape _shape;
        private readonly List<DecorationGroup> _recipes;
        private readonly Random _random;
        private readonly float _minSpacing;

        // minSpacing: distancia minima entre decoraciones del area (0 = sin restriccion)
        public DecorationArea(Shape shape, List<DecorationGroup> recipes, Random random, float minSpacing = 0f)
        {
            _shape = shape;
            _random = random;
            _recipes = recipes;
            _minSpacing = minSpacing;
        }

        public List<Entity> Generate()
        {
            var result = new List<Entity>();

            if (_shape == null) return result;

            foreach (var recipe in _recipes)
            {
                for (int i = 0; i < recipe.Amount; i++)
                {
                    ModelInfo model = recipe.GetRandomModel(_random);
                    Vector3 position = _shape.GetRandomPosition(_random);

                    // Reintenta hasta encontrar un lugar que no quede pegado a otra decoracion
                    if (_minSpacing > 0f)
                    {
                        for (int tries = 0; tries < 60 && IsTooClose(position, result); tries++)
                            position = _shape.GetRandomPosition(_random);

                        if (IsTooClose(position, result)) continue; // no hay lugar: se omite
                    }
                    float rotation = _random.NextSingle() * (float)Math.PI * 2;
                    float scale = 0.75f + _random.NextSingle() * 0.5f;

                    result.Add(DecorationFactory.Create(model, recipe.Type, position, rotation, scale));
                }
            }
            return result;
        }

        private bool IsTooClose(Vector3 position, List<Entity> placed)
        {
            foreach (var e in placed)
            {
                var d = e.Transform.Position - position;
                if (d.X * d.X + d.Z * d.Z < _minSpacing * _minSpacing) return true;
            }
            return false;
        }
    }
}