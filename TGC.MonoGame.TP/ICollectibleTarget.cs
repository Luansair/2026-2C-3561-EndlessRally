

namespace TGC.MonoGame.TP
{
    internal interface ICollectibleTarget
    {
        void AddScore(int points);
        void AddFuel(float amount);
        void AddHealth(float amount);
        void ApplyDamage(float amount);
    }

    // Utilizado para los coleccionables que se consumen al colisionar y desaparecen.
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
