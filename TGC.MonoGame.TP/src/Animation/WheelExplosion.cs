using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TGC.MonoGame.TP.src.Assets;

namespace TGC.MonoGame.TP.src.Animation
{
    /// <summary>
    ///     Animacion "vista explosionada": las ruedas se separan del chasis y giran sobre su eje.
    ///     Cada rueda recibe su propia matriz, generada a partir de la del auto:
    ///
    ///         ruedaWorld = Giro * HuesoAbsoluto * (Desplazamiento * AutoWorld)
    ///
    ///     (MonoGame usa vectores fila: el orden de izquierda a derecha es el orden en que se aplican).
    /// </summary>
    internal class WheelExplosion : IMeshAnimator
    {
        private class Wheel
        {
            public Vector3 Direction;   // hacia donde se aleja, en espacio del modelo
            public bool Spins;          // la de auxilio (montada atras) no gira
            public float Delay;         // escalonado: cada rueda arranca un poco despues
            public float SpinAngle;
        }

        private readonly Dictionary<ModelMesh, Wheel> _wheels = new Dictionary<ModelMesh, Wheel>();
        private readonly float _distance;
        private readonly float _maxDelay;

        /// <summary>0 = ruedas en su lugar, 1 = ruedas totalmente separadas.</summary>
        public float Progress { get; set; }

        /// <summary>Velocidad de giro de las ruedas, en radianes por segundo.</summary>
        public float SpinSpeed { get; set; } = 6f;

        /// <param name="distanceFactor">Cuanto se separan, como fraccion del ancho del auto.</param>
        public WheelExplosion(ModelInfo info, float distanceFactor = 0.8f)
        {
            var bounds = info.LocalBounds;
            var center = (bounds.Min + bounds.Max) * 0.5f;
            float width = bounds.Max.X - bounds.Min.X;
            _distance = width * distanceFactor;

            // 1) Detecto las ruedas por nombre
            var wheelMeshes = new List<ModelMesh>();
            foreach (var mesh in info.Model.Meshes)
            {
                if (IsWheel(mesh)) wheelMeshes.Add(mesh);
            }

            if (wheelMeshes.Count == 0)
            {
                Debug.WriteLine("WheelExplosion: no encontre ruedas. Meshes del modelo:");
                foreach (var mesh in info.Model.Meshes)
                    Debug.WriteLine($"  mesh='{mesh.Name}' bone='{mesh.ParentBone?.Name}'");
                return;
            }

            // 2) Centro longitudinal de las ruedas laterales (las de auxilio quedan fuera del promedio)
            float sumZ = 0f; int lateralCount = 0;
            foreach (var mesh in wheelMeshes)
            {
                var p = info.BoneTransforms[mesh.ParentBone.Index].Translation;
                if (IsLateral(p, center, width)) { sumZ += p.Z; lateralCount++; }
            }
            float centerZ = lateralCount > 0 ? sumZ / lateralCount : center.Z;

            // 3) Direccion de cada rueda a partir de la posicion de su hueso
            int order = 0;
            foreach (var mesh in wheelMeshes)
            {
                var p = info.BoneTransforms[mesh.ParentBone.Index].Translation;
                float sx = Math.Sign(p.X - center.X);
                float sz = Math.Sign(p.Z - centerZ);

                var wheel = new Wheel { Delay = 0.08f * order++ };
                if (IsLateral(p, center, width))
                {
                    wheel.Direction = Vector3.Normalize(new Vector3(sx, 0.3f, sz * 0.5f));
                    wheel.Spins = true;
                }
                else
                {
                    // rueda de auxilio: se despega hacia atras y un poco hacia arriba
                    wheel.Direction = Vector3.Normalize(new Vector3(0f, 0.3f, sz == 0f ? 0f : sz));
                    wheel.Spins = false;
                }
                _wheels[mesh] = wheel;
            }
            _maxDelay = 0.08f * (wheelMeshes.Count - 1);
        }

        public void Update(float dt)
        {
            foreach (var wheel in _wheels.Values)
            {
                if (wheel.Spins)
                    wheel.SpinAngle = MathHelper.WrapAngle(wheel.SpinAngle + SpinSpeed * dt);
            }
        }

        public Matrix GetMeshWorld(ModelMesh mesh, Matrix boneAbsolute, Matrix world)
        {
            if (!_wheels.TryGetValue(mesh, out var wheel))
                return boneAbsolute * world; // carroceria: sin cambios

            float local = MathHelper.Clamp((Progress - wheel.Delay) / (1f - _maxDelay), 0f, 50f);
            float t = MathHelper.SmoothStep(0f, 1f, local);

            // Matriz del "auto" desplazado para esta rueda, generada a partir de la del auto
            Matrix wheelCar = Matrix.CreateTranslation(wheel.Direction * _distance * t) * world;

            // Giro sobre el eje de la rueda (el origen del mesh es el centro de la rueda y su eje es X)
            Matrix spin = wheel.Spins ? Matrix.CreateRotationX(wheel.SpinAngle) : Matrix.Identity;

            return spin * boneAbsolute * wheelCar;
        }

        // Rueda "lateral" = no esta sobre el plano central del auto
        private static bool IsLateral(Vector3 bonePos, Vector3 center, float width)
            => Math.Abs(bonePos.X - center.X) > width * 0.25f;

        private static bool IsWheel(ModelMesh mesh)
        {
            return NameHasWheel(mesh.Name) || NameHasWheel(mesh.ParentBone?.Name);
        }

        private static bool NameHasWheel(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("wheel", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("rueda", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
