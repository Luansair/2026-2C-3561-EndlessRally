using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.src.Animation;
using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Rendering;

namespace TGC.MonoGame.TP.src.Collectibles
{
    internal class CollectibleFactory
    {
        private readonly CollectibleAssets _assets;
        public CollectibleFactory(CollectibleAssets assets)
        {
            _assets = assets;
        }

        public Entity CreateRandom(Vector3 position, Random random)
        {
            int roll = random.Next(100);

            ModelInfo model;
            CollectibleEffect effect;
            Vector3 collPos = position;

            if (roll < 45) // 45% Moneda/Gema
            {
                model = _assets.Coin;
                effect = new CollectibleEffect { Score = 10 };
                collPos.Y -= 1f; 
            }
            else if (roll < 75) // 30% Nafta
            {
                model = _assets.Fuel;
                effect = new CollectibleEffect { Fuel = 50.0f };
                collPos.Y -= 1f; 
            }
            else if (roll < 85) // 10% Llave inglesa (Reparación)
            {
                model = _assets.Wrench;
                effect = new CollectibleEffect { Health = 40.0f };
                collPos.Y -= 0.5f; 
            }
            else // 15% Obstáculo / Trampa
            {
                model = _assets.Obstacle;
                effect = new CollectibleEffect { Damage = 25.0f };
                collPos.Y -= 1f; // Ajuste de posición para que esté en el suelo
            }

            var collectible = new Entity
            {
                Render = new RenderModel(model),
                Collider = new Collider(AABBShape.FromModel(model), isTrigger: true),
                Collectible = effect,
                Response = NullCollisionResponse.Instance,
            };
            collectible.Render.Animator = new TransformAnimator
            {
                RotationSpeed = 2f,     // qué tan rápido gira
                BobbingAmplitude = 0.7f, // qué tanto sube y baja
                BobbingSpeed = 2.0f       // frecuencia del rebote
            };
            collectible.Transform.Scale = model.Scale;
            collectible.Transform.Position = collPos;
            collectible.isCollectible = true;

            return collectible;
        }
    }
}
