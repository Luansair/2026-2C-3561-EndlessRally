using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.src.Core
{
    internal class Transform
    {
        private Vector3 _position;
        public float _rotationY;
        private float _scale = 1.0f;
        private Matrix _world;
        private bool _dirty = true;

        public Vector3 Position { get => _position; set { _position = value; _dirty = true; } }
        public float RotationY { get => _rotationY; set { _rotationY = value; _dirty = true; } }
        public float Scale { get => _scale; set { _scale = value; _dirty = true; } }

        public Matrix World
        {
            get
            {
                if (_dirty)
                {
                    _world = Matrix.CreateScale(_scale) * Matrix.CreateRotationY(_rotationY) * Matrix.CreateTranslation(_position);
                    _dirty = false;
                }
                return _world;
            }
        }
    }
}