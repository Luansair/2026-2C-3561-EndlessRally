using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace TGC.MonoGame.TP
{
    internal class DecorationArea
    {
        private readonly Shape _shape;
        private readonly List<Decoration> _decorations;
        private readonly Random _random;

        public DecorationArea(Shape shape, List<DecorationGroup> recipes, Random random)
        {
            _shape = shape;
            _random = random;
            _decorations = new List<Decoration>();

            foreach (DecorationGroup recipe in recipes)
            {
                GenerateDecorations(recipe);
            }
        }

        private void GenerateDecorations(DecorationGroup recipe)
        {
            for (int i = 0; i < recipe.Amount; i++)
            {
                ModelInfo model = recipe.GetRandomModel(_random);
                Vector3 position = _shape.GetRandomPosition(_random);
                float rotation = _random.NextSingle() * (float)Math.PI * 2;
                float scale = 0.75f + _random.NextSingle() * 0.5f;
                
                Decoration decoration = new Decoration(model, recipe.Type, position, rotation, scale);

                _decorations.Add(decoration);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            foreach (Decoration decoration in _decorations)
            {
                decoration.Draw(graphicsDevice, effect, view, projection);
            }
        }

        public void DrawRelativeTo(Matrix parentWorld, Effect effect, Matrix view, Matrix projection)
        {
            foreach (Decoration decoration in _decorations)
            {
                decoration.DrawRelativeTo(parentWorld, effect, view, projection);
            }
        }
    }
}
