using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.src.Collectibles;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Rendering;
using TGC.MonoGame.TP.src.Vehicles;

namespace TGC.MonoGame.TP.src.Core
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

        // Solo para los tramos de camino: mezcla de superficies y largo del tramo (para el shader)
        public SurfaceBlend? Blend { get; set; }
        public float ChunkLength { get; set; }

        public Entity() { }
        public BoundingSphere GetWorldSphere()
        {
            var info = Render?.ModelInfo;
            if (info == null) return new BoundingSphere(Transform.Position, 30f);

            BoundingBox b = info.LocalBounds;
            Vector3 localCenter = (b.Min + b.Max) * 0.5f;
            float localRadius = (b.Max - b.Min).Length() * 0.5f;

            return new BoundingSphere(
                Vector3.Transform(localCenter, Transform.World),
                localRadius * Transform.Scale);
        }
        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            if (Render == null) return;
            Matrix w = Transform.World;

            if (Blend.HasValue)
            {
                var b = Blend.Value;
                // Eje local +Z del tramo en mundo (Matrix.Forward es -Z, por eso TransformNormal)
                Vector3 forward = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, w));
                effect.Parameters["ChunkOrigin"]?.SetValue(w.Translation);
                effect.Parameters["ChunkForward"]?.SetValue(forward);
                effect.Parameters["ChunkLength"]?.SetValue(ChunkLength);
                effect.Parameters["SurfaceTypeA"]?.SetValue((float)b.From);
                effect.Parameters["SurfaceTypeB"]?.SetValue((float)b.To);
                effect.Parameters["BlendStart"]?.SetValue(b.T0);
                effect.Parameters["BlendEnd"]?.SetValue(b.T1);
            }

            Render.Draw(effect, view, projection, w);

            // Apagamos la mezcla para lo que se dibuja despues
            if (Blend.HasValue) effect.Parameters["UseSurfaceBlend"]?.SetValue(0f);
        }
    }
    
}
