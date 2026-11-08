using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.src.Animation
{
    internal class TransformAnimator : IMeshAnimator
    {
        private float _currentRotationY = 0f;
        private float _timeElapsed = 0f;

        /// <summary>Velocidad de rotación en radianes por segundo.</summary>
        public float RotationSpeed { get; set; } = 2f;

        /// <summary>Altura máxima que sube y baja (amplitud).</summary>
        public float BobbingAmplitude { get; set; } = 5f;

        /// <summary>Velocidad del movimiento de arriba y abajo.</summary>
        public float BobbingSpeed { get; set; } = 4f;

        public void Update(float dt)
        {
            _timeElapsed += dt;
            // Actualiza la rotación acumulada
            _currentRotationY = MathHelper.WrapAngle(_currentRotationY + RotationSpeed * dt);
        }

        public Matrix GetMeshWorld(ModelMesh mesh, Matrix boneAbsolute, Matrix world)
        {
            // 1. Rotación sobre su propio eje (asumiendo eje Y)
            Matrix rotation = Matrix.CreateRotationY(_currentRotationY);

            // 2. Movimiento vertical (arriba y abajo) usando una onda senoidal
            float verticalOffset = (float)Math.Sin(_timeElapsed * BobbingSpeed) * BobbingAmplitude;
            Matrix translation = Matrix.CreateTranslation(0f, verticalOffset, 0f);

            // Orden: Primero rota en su lugar, luego se desplaza verticalmente, 
            // y finalmente se multiplica por el hueso y la matriz de mundo general.
            return rotation * translation * boneAbsolute * world;
        }
    }
}