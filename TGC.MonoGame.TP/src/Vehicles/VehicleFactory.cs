using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Rendering;

namespace TGC.MonoGame.TP.src.Vehicles
{
    internal class VehicleFactory
    {
        public static Entity Create(ModelInfo model, float health, float fuel, float fuelConsumption, float turnSpeed, float acceleration)
        {
            var vehicle = new Entity
            {
                Render = new RenderModel(model),
                Collider = new Collider(AABBShape.FromModel(model)),
                Damageable = new Damageable(health),
                Fuel = new Fuel(fuel),
                VehicleStats = new VehicleStats 
                {
                    FuelConsumption = fuelConsumption,
                    TurnSpeed = turnSpeed,
                    Acceleration = acceleration
                },
                Active = true,
                IsMovable = true
            };
            vehicle.Transform.Scale = model.Scale;
            return vehicle;
        }
    }
}
