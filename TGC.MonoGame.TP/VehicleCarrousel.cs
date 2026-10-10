using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class VehicleCarrousel
    {
        private List<Entity> Vehicles = new List<Entity>();
        private int CurrentIndex = 0;

        public void Add(Entity vehicle)
        {
            // Check que es vehiculo
            if (vehicle == null) return;
            Vehicles.Add(vehicle);
        }
        public Entity GetCurrent() => Vehicles[CurrentIndex];
        public Entity GetNext()
        {
            if (Vehicles == null || Vehicles.Count == 0)
                return null;

            CurrentIndex = (CurrentIndex + 1) % Vehicles.Count;
            return Vehicles[CurrentIndex];
        }
        public Entity GetPrev()
        {
            if (Vehicles == null || Vehicles.Count == 0)
                return null;
            CurrentIndex = (CurrentIndex - 1 + Vehicles.Count) % Vehicles.Count;
            return Vehicles[CurrentIndex];
        }
    }
}
