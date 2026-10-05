using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    internal class CollectibleFactory
    {
        private readonly CollectibleAssets _collectibleAssets;
        public CollectibleFactory(CollectibleAssets collectibleAssets)
        {
            _collectibleAssets = collectibleAssets;
        }

        public Collectible CreateRandom(Vector3 position, Random random)
        {
            int roll = random.Next(100);
            Collectible nuevo;

            if (roll < 55) // 45% Moneda/Gema
                nuevo = new FichaCollectible(_collectibleAssets.Coin, position - new Vector3(0f, 1f, 0f), 10);
            else if (roll < 70) // 25% Nafta
                nuevo = new FuelCollectible(_collectibleAssets.Fuel, position, 25f);
            else if (roll < 85) // 15% Llave inglesa (Reparación)
                nuevo = new WrenchCollectible(_collectibleAssets.Wrench, position, 20f);
            else // 15% Obstáculo / Trampa
                nuevo = new DamageCollectible(_collectibleAssets.Obstacle, position - new Vector3(0f, 1f, 0f), 15f);

            return nuevo;
        }
    }
}
