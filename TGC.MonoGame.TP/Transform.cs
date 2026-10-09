using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    internal class Transform
    {
        public Vector3 Position;
        public float RotationY;
        public float Scale = 1.0f;

        public Matrix World => 
            Matrix.CreateScale(Scale) *
            Matrix.CreateRotationY(RotationY) *
            Matrix.CreateTranslation(Position);
    }
}
