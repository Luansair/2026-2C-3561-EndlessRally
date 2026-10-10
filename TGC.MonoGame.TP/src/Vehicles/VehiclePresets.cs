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
            VehicleType.LightCar => VehicleFactory.Create(_assets.LightCar, 150, 150, 10, 100, 200),
            VehicleType.MediumCar => VehicleFactory.Create(_assets.MediumCar, 250, 200, 15, 70, 120),
            VehicleType.HeavyCar => VehicleFactory.Create(_assets.HeavyCar, 500, 300, 50, 30, 80),
            _ => null
        };
    }
}
