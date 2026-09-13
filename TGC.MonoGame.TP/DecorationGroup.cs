using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class DecorationGroup
    {
        public DecorationType Type { get; }
        public int Amount { get; }
        private List<ModelInfo> _models;

        public DecorationGroup(DecorationType type, int amount, List<ModelInfo> models)
        {
            this.Type = type;
            this.Amount = amount;
            this._models = models;
        }

        public void AddModel(ModelInfo model)
        {
            _models.Add(model);
        }

        public ModelInfo GetRandomModel(Random random)
        {
            int index = random.Next(0, _models.Count);
            return _models[index];
        }

    }
}
