using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using TGC.MonoGame.TP.src.Collectibles;

using TGC.MonoGame.TP.src.Road;
namespace TGC.MonoGame.TP.src.Core
{
    internal class TrafficController 
    {
        public List<TrafficCar> Vehicles { get; }
       
        public TrafficController(List<TrafficCar> vehicles) { Vehicles = vehicles; }
        public bool godmode = false;

        
        public void Update(GameTime gameTime, RoadSpawner roadSpawner)
        {
            foreach (var vehicle in Vehicles)
            {
                Func<float, Vector3> path = roadSpawner.CreateTrafficPath(vehicle.Vehicle.Transform.Position);

                if (path != null)
                {
                    vehicle.Update(gameTime, path);
                }else
                {
                    vehicle.Update(gameTime, _ => new Vector3(0, 0, -5f));
                }
            }
        }

    }
}
