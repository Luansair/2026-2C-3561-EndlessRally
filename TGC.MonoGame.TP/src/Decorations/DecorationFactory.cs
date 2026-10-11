using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Road;
using TGC.MonoGame.TP.src.Rendering;

namespace TGC.MonoGame.TP.src.Decorations
{
    public enum DecorationType
    {
        Tree,
        Rock,
        Bush,
        Flower,
        House
    }

    internal class DecorationFactory
    {
        public static Entity Create(ModelInfo model, DecorationType type, Vector3 position, float rotation, float scale)
        {
            var entity = new Entity
            {
                Render = new RenderModel(model) { FallbackColor = GetFallbackColor(type) },
                Collider = new Collider(AABBShape.FromModel(model), isTrigger: false),
                Response = new DamageResponse(GetDamage(type)),
                IsMovable = false,
            };
            entity.Transform.Position = position;
            entity.Transform.RotationY = rotation;
            entity.Transform.Scale = scale * model.Scale;

            return entity;
        }

        // Método auxiliar para colores de respaldo cuando el FBX no define materiales válidos
        private static Vector3 GetFallbackColor(DecorationType type) => type switch
        {
            DecorationType.Tree => new Vector3(0.20f, 0.65f, 0.20f),    // Verde hoja
            DecorationType.Rock => new Vector3(0.65f, 0.65f, 0.68f),    // Gris piedra
            DecorationType.Bush => new Vector3(0.15f, 0.55f, 0.15f),    // Verde arbusto
            DecorationType.Flower => new Vector3(0.95f, 0.30f, 0.35f),  // Tono claro flor
            DecorationType.House => new Vector3(0.85f, 0.80f, 0.75f),   // beige
            _ => Vector3.One
        };

        private static float GetDamage(DecorationType type) => type switch
        {
            DecorationType.Tree => 20f,
            DecorationType.Rock => 15f,
            DecorationType.Bush => 5f,
            DecorationType.Flower => 0f,
            DecorationType.House => 30f,
            _ => 0f
        };
    }
}
