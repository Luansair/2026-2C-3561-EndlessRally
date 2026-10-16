using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Vehicles
{
    public enum VehicleType {
        LightCar,
        MediumCar,
        HeavyCar
    }
    internal class VehiclePresets
    {
        private readonly CarAssets _assets;

        public VehiclePresets(CarAssets assets)
        {
            _assets = assets;
        }

        public Entity Create(VehicleType type) => type switch
        {
            VehicleType.LightCar => VehicleFactory.Create(_assets.LightCar, 150f, 150f, 10f* 0.01f, 100f, 80f),
            VehicleType.MediumCar => VehicleFactory.Create(_assets.MediumCar, 250f, 200f, 15f * 0.01f, 70f, 60f),
            VehicleType.HeavyCar => VehicleFactory.Create(_assets.HeavyCar, 500f, 300f, 50f * 0.01f, 40f, 40f),
            _ => null
        };
    }
}
