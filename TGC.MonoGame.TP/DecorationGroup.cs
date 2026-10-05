using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class DecorationGroup
    {
        public DecorationType Type { get; }
        public int Amount { get; }
        private IReadOnlyList<ModelInfo> _models;

        public DecorationGroup(DecorationType type, int amount, IReadOnlyList<ModelInfo> models)
        {
            this.Type = type;
            this.Amount = amount;
            this._models = models;
        }

        public ModelInfo GetRandomModel(Random random)
        {
            int index = random.Next(0, _models.Count);
            return _models[index];
        }

    }
}
