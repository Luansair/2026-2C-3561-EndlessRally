using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
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

            if (roll < 55) // 45% Moneda/Gema
            {
                model = _assets.Coin;
                effect = new CollectibleEffect { Score = 10 };
                collPos -= new Vector3(0f, 1f, 0f); // Ajuste de posición para que esté en el suelo
            }
            else if (roll < 70) // 25% Nafta
            {
                model = _assets.Fuel;
                effect = new CollectibleEffect { Fuel = 50.0f };
            }
            else if (roll < 85) // 15% Llave inglesa (Reparación)
            {
                model = _assets.Wrench;
                effect = new CollectibleEffect { Health = 40.0f };
            }
            else // 15% Obstáculo / Trampa
            {
                model = _assets.Obstacle;
                effect = new CollectibleEffect { Damage = 25.0f };
                collPos -= new Vector3(0f, 1f, 0f); // Ajuste de posición para que esté en el suelo
            }

            var collectible = new Entity
            {
                Render = new RenderModel(model),
                Collider = new Collider(AABBShape.FromModel(model), isTrigger: true),
                Collectible = effect,
                Response = NullCollisionResponse.Instance,
            };

            collectible.Transform.Position = collPos;
            collectible.Transform.Scale = model.Scale;

            return collectible;
        }
    }
}
