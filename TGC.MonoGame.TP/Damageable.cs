using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP
{
    internal class Damageable
    {
        public float Current { get; private set; }
        public float Max { get; }

        public Damageable(float max)
        {
            Max = max;
            Current = max;
        }

        public void ApplyDamage(float amount) => Current = Math.Max(Current - amount, 0);
        public void Heal(float amount) => Current = Math.Min(Current + amount, Max);
        public bool IsDead => Current <= 0;
    }
}
