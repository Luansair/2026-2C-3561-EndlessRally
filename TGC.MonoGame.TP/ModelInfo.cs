using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public readonly struct ModelInfo
    {
        public readonly Model Model { get; }
        public readonly float Scale { get; }
        public ModelInfo(Model model, float scale)
        {
            Model = model;
            Scale = scale;
        }
    }
}
