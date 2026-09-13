using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     This structure stores the model and its scale for normalization, which means everytime the model is drawn, it should be scaled by this factor.
    /// </summary>
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
