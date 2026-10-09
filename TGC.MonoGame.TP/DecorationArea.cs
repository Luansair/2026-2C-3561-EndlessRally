using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace TGC.MonoGame.TP
{
    internal class DecorationArea
    {
        private readonly Shape _shape;
        private readonly List<DecorationGroup> _recipes;
        private readonly Random _random;

        public DecorationArea(Shape shape, List<DecorationGroup> recipes, Random random)
        {
            _shape = shape;
            _random = random;
            _recipes = recipes;
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
                    float rotation = _random.NextSingle() * (float)Math.PI * 2;
                    float scale = 0.75f + _random.NextSingle() * 0.5f;

                    result.Add(DecorationFactory.Create(model, recipe.Type, position, rotation, scale));
                }
            }
            return result;
        }
    }
}
