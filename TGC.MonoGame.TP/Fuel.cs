using System;

namespace TGC.MonoGame.TP
{
    internal class Fuel
    {
        public float Current { get; private set; }
        public float Max { get; }
        public Fuel(float max)
        {
            Max = max;
            Current = max;
        }

        public void Consume(float amount)
        {
            Current = Math.Max(0, Current - amount);
        }

        public void Refill(float amount)
        {
            Current = Math.Min(Max, Current + amount);
        }
    }
}
