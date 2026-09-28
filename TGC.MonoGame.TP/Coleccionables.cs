using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace TGC.MonoGame.TP
{
    public abstract class Collectible
    {
        public ModelInfo modelI;
        public readonly Vector3 pos;
        public BoundingBox hitbox { get; private set; }

        public bool Collected { get; private set; }

        protected Collectible(ModelInfo model, Vector3 pos)
        {
            this.modelI = model; 
            this.pos = pos;
            this.hitbox = GizmoPrimitives.CreateAABBFrom(model.Model);
            //this.hitbox = GizmoPrimitives.TransformAABB(GizmoPrimitives.CreateAABBFrom(model.Model), Matrix.CreateScale(modelI.Scale) * Matrix.CreateTranslation(pos));
            this.Collected = false;
        }

        //esto hay que pasarlo en la colision 

        public void tryCollect(Vehiculo vehiculo)
        {
            if (!this.checkCollision(vehiculo.hitboxWorld) || Collected) return;
            apply(vehiculo);
            Collected = true;
        }
        public bool checkCollision(BoundingBox vehiculoHitboxWorld)
        {
            var world = Matrix.CreateScale(modelI.Scale) * Matrix.CreateTranslation(pos);
            var corners = hitbox.GetCorners();
            for (int i = 0; i < corners.Length; i++)
                corners[i] = Vector3.Transform(corners[i], world);

            var hitboxWorld = BoundingBox.CreateFromPoints(corners);
            return hitboxWorld.Intersects(vehiculoHitboxWorld);
        }
        public abstract void apply(Vehiculo vehiculo);

        public void Draw(Effect effect, Matrix view, Matrix projection,GameTime gameTime)
        {
            if (!Collected) {
                Matrix world = Matrix.CreateScale(modelI.Scale) *
                          Matrix.CreateRotationY((float)gameTime.ElapsedGameTime.TotalSeconds) *
                          Matrix.CreateTranslation(pos);

                foreach (ModelMesh mesh in modelI.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effect;
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());
                    }
                    mesh.Draw();
                }
            }
        }
    }

    public class FichaCollectible : Collectible
    {
        private readonly int points;
        public FichaCollectible(ModelInfo model, Vector3 pos, int points) : base(model, pos) => this.points = points;
        public override void apply(Vehiculo v) => v.score += points;
    }

    public class FuelCollectible : Collectible
    {
        private readonly float fuel;
        public FuelCollectible(ModelInfo model, Vector3 pos, float amount) : base(model, pos) => fuel = amount;
        public override void apply(Vehiculo v) => v.currentFuel = Math.Min(v.currentFuel + fuel, v.stats.maxFuel);
    }

    public class WrenchCollectible : Collectible
    {
        private readonly float hp;
        public WrenchCollectible(ModelInfo model, Vector3 pos, float amount) : base(model, pos) => hp = amount;
        public override void apply(Vehiculo v) => v.currentHealth = Math.Min(v.currentHealth + hp, v.stats.maxHealth);
    }

    public class DamageCollectible : Collectible
    {
        private readonly float damage;
        public DamageCollectible(ModelInfo model, Vector3 pos, float amount) : base(model, pos) => damage = amount;
        public override void apply(Vehiculo v) => v.currentHealth = Math.Max(v.currentHealth - damage, 0);
    }
}
