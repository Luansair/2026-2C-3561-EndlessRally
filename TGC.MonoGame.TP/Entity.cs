using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    internal class Entity
    {
        public Transform Transform { get; } = new Transform();
        public RenderModel Render { get; set; }
        public Collider Collider { get; set; } = Collider.None;
        public ICollisionResponse Response { get; set; } = NullCollisionResponse.Instance;
        public SurfaceMaterial Surface { get; set; }
        public Damageable Damageable { get; set; }
        public Fuel Fuel { get; set; }
        public VehicleStats VehicleStats { get; set; }
        public float Speed { get; set; } = 0f;
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 Acceleration { get; set; } = Vector3.Zero;
        public CollectibleEffect Collectible { get; set; }
        public bool Active { get; set; } = true;
        public bool IsMovable { get; set; } = false;

        public Entity() { }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            if (Render == null) return;
            Render.Draw(effect, view, projection, Transform.World);
        }
    }
    
}
