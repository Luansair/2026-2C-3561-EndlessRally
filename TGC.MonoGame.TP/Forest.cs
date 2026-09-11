using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class Forest
    {
        private List<Tree> _trees;
        private Vector3 _center;
        private float _radius;
        private Random _seed;
        private ModelInfo[] _treeModels;

        public Forest(ModelInfo[] tree_models, Vector3 center, float radius, int tree_count, Random seed)
        {
            _treeModels = tree_models;
            _center = center;
            _radius = radius;
            _seed = seed;
            _trees = new List<Tree>();

            for (int i = 0; i < tree_count; i++ )
            {
                ModelInfo tree_model_picked = _treeModels[i % _treeModels.Length];
                Vector3 tree_position = new Vector3(
                    _center.X + (float)(_seed.NextDouble() * 2 - 1) * _radius,
                    _center.Y,
                    _center.Z + (float)(_seed.NextDouble() * 2 - 1) * _radius
                );
                float tree_rotation = (float)(_seed.NextDouble() * Math.PI * 2);
                float tree_scale = tree_model_picked.Scale * (float)(_seed.NextDouble() * 0.5 + 0.75);
                Tree tree = new Tree(tree_model_picked.Model, tree_position, tree_rotation, tree_scale);
                _trees.Add(tree);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            foreach (Tree tree in _trees)
            {
                tree.Draw(graphicsDevice, effect, view, projection);
            }
        }
    }
}
