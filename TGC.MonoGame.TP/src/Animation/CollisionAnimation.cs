using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TGC.MonoGame.TP.src.Assets;

namespace TGC.MonoGame.TP.src.Animation
{
    /// <summary>
    ///     Animacion breve de impacto: el modelo se sacude y rota alrededor de su centro.
    /// </summary>
    internal class CollisionAnimation : IMeshAnimator
    {
        private const float AnimationDuration = 0.6f;
        private const float RotationAmplitude = 0.12f;

        private readonly Vector3 _center;
        private readonly float _translationAmplitude;
        private Vector3 _impactDirection = Vector3.UnitX;
        private long _startTimestamp;
        private bool _crash;
        private bool _collect;

        public bool IsPlaying => (_crash || _collect) && GetProgress() < 1f;

        public CollisionAnimation(ModelInfo info)
        {
            var bounds = info.LocalBounds;
            _center = (bounds.Min + bounds.Max) * 0.5f;
            var size = bounds.Max - bounds.Min;
            _translationAmplitude = Math.Max(size.X, Math.Max(size.Y, size.Z)) * 0.025f;
        }

        /// <summary>Inicia el impacto en una direccion expresada en el espacio del modelo.</summary>
        public void StartCrash(Vector3 impactDirection)
        {
            if (impactDirection.LengthSquared() <= float.Epsilon)
                throw new ArgumentException("La direccion del impacto no puede ser cero.", nameof(impactDirection));

            _impactDirection = Vector3.Normalize(impactDirection);
            _startTimestamp = Stopwatch.GetTimestamp();
            _collect = false;
            _crash = true;
        }

        /// <summary>Inicia la animacion de recogida del objeto.</summary>
        public void StartCollect()
        {
            _startTimestamp = Stopwatch.GetTimestamp();
            _crash = false;
            _collect = true;
        }

        public Matrix GetMeshWorld(ModelMesh mesh, Matrix boneAbsolute, Matrix world)
        {
            if (!(_crash || _collect)) return boneAbsolute * world;

            if (_crash) { return GetMeshWorldCrash(mesh, boneAbsolute, world); }
            else { return GetMeshWorldCollect(mesh, boneAbsolute, world); }
        }

        private Matrix GetMeshWorldCrash(ModelMesh mesh, Matrix boneAbsolute, Matrix world)
        {
            float progress = GetProgress();
            if (progress >= 1f) 
            {
                _crash = false;
                return boneAbsolute * world;
            }

            float decay = 1f - progress;
            float oscillation = (float)Math.Sin(progress * MathHelper.TwoPi * 3f) * decay;
            var translation = _impactDirection * (_translationAmplitude * oscillation);
            float angle = RotationAmplitude * oscillation;

            Matrix rotation = Matrix.CreateRotationZ(_impactDirection.X * angle)
                * Matrix.CreateRotationY(-_impactDirection.Z * angle);
            Matrix impact = Matrix.CreateTranslation(-_center)
                * rotation
                * Matrix.CreateTranslation(_center + translation);

            return boneAbsolute * impact * world;
        }

        private Matrix GetMeshWorldCollect(ModelMesh mesh, Matrix boneAbsolute, Matrix world)
        {
            float progress = GetProgress();
            if (progress >= 1f)
            {
                _collect = false;
                return boneAbsolute * world;
            }

            float fade = 1f - MathHelper.SmoothStep(0f, 1f, progress);
            float phase = progress * MathHelper.TwoPi * 12f
                + (mesh.ParentBone?.Index ?? 0) * 0.7f;
            float amplitude = _translationAmplitude * 0.5f * fade;

            var vibration = new Vector3(
                (float)Math.Sin(phase),
                (float)Math.Sin(phase * 1.3f),
                (float)Math.Cos(phase * 0.9f)) * amplitude;
            float angle = RotationAmplitude * 0.25f * fade;

            Matrix rotation = Matrix.CreateRotationX((float)Math.Sin(phase) * angle)
                * Matrix.CreateRotationZ((float)Math.Cos(phase) * angle);
            Matrix collect = Matrix.CreateTranslation(-_center)
                * rotation
                * Matrix.CreateTranslation(_center + vibration);

            return boneAbsolute * collect * world;
        }

        private float GetProgress()
        {
            double elapsed = (Stopwatch.GetTimestamp() - _startTimestamp) / (double)Stopwatch.Frequency;
            return MathHelper.Clamp((float)(elapsed / AnimationDuration), 0f, 1f);
        }
    }
}
