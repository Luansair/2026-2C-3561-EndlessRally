using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.src.Collectibles
{
    internal class CollectibleEffect
    {
        public int Score;
        public float Fuel;
        public float Health;
        public float Damage;

        public void ApplyTo(ICollectibleTarget target)
        {
            if (Score != 0) target.AddScore(Score);
            if (Fuel != 0) target.AddFuel(Fuel);
            if (Health != 0) target.AddHealth(Health);
            if (Damage != 0) target.ApplyDamage(Damage);
        }
    }
}
