
namespace TGC.MonoGame.TP
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

            return vehicle;
        }
    }
}
