namespace TGC.MonoGame.TP.src.Collectibles
{
    internal interface ICollectibleTarget
    {
        void AddScore(int points);
        void AddFuel(float amount);
        void AddHealth(float amount);
        void ApplyDamage(float amount);
    }
}
