using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using TGC.MonoGame.TP.src.Collectibles;

namespace TGC.MonoGame.TP.src.Core
{
internal class TrafficCar
{
    public Entity Vehicle { get; }
    private float dificultad = 0.71f;

    public TrafficCar(Entity vehicle) 
    { 
        Vehicle = vehicle;
    }

    public void Update(GameTime gameTime, Func<float, Vector3> _path)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var t = Vehicle.Transform;

        dificultad += 0.01f * dt;
        float step = Vehicle.VehicleStats.Acceleration * dt * dificultad;

        float d = -t.Position.Z;
        Vector3 delta = _path(d + step) - _path(d);

        t.Position += delta;

        if (delta != Vector3.Zero)
            t.RotationY = MathF.Atan2(-delta.X, -delta.Z);
    }
}
} 